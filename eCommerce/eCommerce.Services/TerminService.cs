using eCommerce.Common.Services.Messaging;
using eCommerce.Common.Services.Realtime;
using eCommerce.Model.Exceptions;
using eCommerce.Model.Requests;
using eCommerce.Model.Responses;
using eCommerce.Model.SearchObjects;
using eCommerce.Services.Database;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Services
{
    /// <summary>
    /// Termin ima state machine status: Zakazan -> Potvrdjen -> Odradjen
    ///                                                   \-> Otkazan / NijeSeOdazvao
    /// Prelazi se rade preko posebnih akcija (Potvrdi/Otkazi/...), ne preko generickog Update-a.
    /// </summary>
    public class TerminService : BaseCRUDService<Termin, TerminResponse, TerminSearchObject, TerminInsertRequest, TerminUpdateRequest>, ITerminService
    {
        // Dozvoljeni prelazi stanja
        private static readonly Dictionary<TerminStatus, TerminStatus[]> DozvoljeniPrelazi = new()
        {
            [TerminStatus.Zakazan] = new[] { TerminStatus.Potvrdjen, TerminStatus.Otkazan },
            [TerminStatus.Potvrdjen] = new[] { TerminStatus.Odradjen, TerminStatus.Otkazan, TerminStatus.NijeSeOdazvao },
            [TerminStatus.Odradjen] = Array.Empty<TerminStatus>(),
            [TerminStatus.Otkazan] = Array.Empty<TerminStatus>(),
            [TerminStatus.NijeSeOdazvao] = Array.Empty<TerminStatus>(),
        };

        private readonly IRabbitMqPublisher _rabbitMqPublisher;
        private readonly IRealtimeNotifier _realtimeNotifier;

        public TerminService(ECommerceDbContext dbContext, MapsterMapper.IMapper mapper, IValidator<TerminInsertRequest> insertValidator, IValidator<TerminUpdateRequest> updateValidator, IRabbitMqPublisher rabbitMqPublisher, IRealtimeNotifier realtimeNotifier) : base(dbContext, mapper, insertValidator, updateValidator)
        {
            _rabbitMqPublisher = rabbitMqPublisher;
            _realtimeNotifier = realtimeNotifier;
        }

        protected override async Task<IQueryable<Termin>> IncludeRelatedEntitiesAsync(TerminSearchObject? search, IQueryable<Termin> query = null)
        {
            query = query.Include(t => t.Klijent).Include(t => t.Frizer).ThenInclude(f => f.User).Include(t => t.Usluga);
            return await Task.FromResult(query);
        }

        protected override IEnumerable<Termin> ApplyFilters(IEnumerable<Termin> query, TerminSearchObject? search)
        {
            if (search != null)
            {
                if (search.KlijentId.HasValue)
                    query = query.Where(t => t.KlijentId == search.KlijentId.Value);

                if (search.FrizerId.HasValue)
                    query = query.Where(t => t.FrizerId == search.FrizerId.Value);

                if (search.UslugaId.HasValue)
                    query = query.Where(t => t.UslugaId == search.UslugaId.Value);

                if (search.Status.HasValue)
                    query = query.Where(t => (int)t.Status == search.Status.Value);

                if (search.OdDatuma.HasValue)
                    query = query.Where(t => t.DatumVrijeme >= search.OdDatuma.Value);

                if (search.DoDatuma.HasValue)
                    query = query.Where(t => t.DatumVrijeme <= search.DoDatuma.Value);
            }

            return query;
        }

        public override async Task<TerminResponse> InsertAsync(TerminInsertRequest request)
        {
            var validationResult = await _insertValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => _mapper.Map<ValidationFailure>(e));
                throw new FluentValidation.ValidationException(errors);
            }

            var usluga = await _dbContext.Usluge.FindAsync(request.UslugaId);
            if (usluga == null || !usluga.IsActive)
                throw new ClinetException("Odabrana usluga ne postoji ili nije aktivna.");

            var frizer = await _dbContext.Frizeri.FindAsync(request.FrizerId);
            if (frizer == null || !frizer.IsActive)
                throw new ClinetException("Odabrani frizer ne postoji ili nije aktivan.");

            var klijent = await _dbContext.Users.FindAsync(request.KlijentId);
            if (klijent == null)
                throw new ClinetException("Odabrani klijent ne postoji.");

            var frizerRadiUslugu = await _dbContext.FrizerUsluge.AnyAsync(fu => fu.FrizerId == request.FrizerId && fu.UslugaId == request.UslugaId);
            if (!frizerRadiUslugu)
                throw new ClinetException("Odabrani frizer ne izvodi odabranu uslugu.");

            var trajanje = usluga.TrajanjeMinuta;
            var pocetak = request.DatumVrijeme;
            var kraj = pocetak.AddMinutes(trajanje);

            var aktivniStatusi = new[] { TerminStatus.Zakazan, TerminStatus.Potvrdjen };

            var preklapanje = await _dbContext.Termini.AnyAsync(t =>
                t.FrizerId == request.FrizerId &&
                aktivniStatusi.Contains(t.Status) &&
                pocetak < t.DatumVrijeme.AddMinutes(t.TrajanjeMinuta) &&
                kraj > t.DatumVrijeme);

            if (preklapanje)
                throw new ClinetException("Frizer vec ima zakazan termin u tom vremenskom periodu.");

            var termin = new Termin
            {
                KlijentId = request.KlijentId,
                FrizerId = request.FrizerId,
                UslugaId = request.UslugaId,
                DatumVrijeme = request.DatumVrijeme,
                TrajanjeMinuta = trajanje,
                Cijena = usluga.Cijena,
                Status = TerminStatus.Zakazan,
                Napomena = request.Napomena,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Termini.Add(termin);
            await _dbContext.SaveChangesAsync();

            await KreirajNotifikacijuAsync(termin, "Termin zakazan", $"Vas termin za '{usluga.Naziv}' je zakazan za {termin.DatumVrijeme:dd.MM.yyyy HH:mm}.", NotifikacijaTip.Opsta);

            return await GetByIdAsync(termin.Id);
        }

        public async Task<TerminResponse> PotvrdiAsync(int id) => await PromijeniStatusAsync(id, TerminStatus.Potvrdjen, "Termin potvrdjen", NotifikacijaTip.TerminPotvrdjen);

        public async Task<TerminResponse> OtkaziAsync(int id) => await PromijeniStatusAsync(id, TerminStatus.Otkazan, "Termin otkazan", NotifikacijaTip.TerminOtkazan);

        public async Task<TerminResponse> OznaciOdradjenAsync(int id) => await PromijeniStatusAsync(id, TerminStatus.Odradjen, "Termin odradjen", NotifikacijaTip.Opsta);

        public async Task<TerminResponse> OznaciNijeSeOdazvaoAsync(int id) => await PromijeniStatusAsync(id, TerminStatus.NijeSeOdazvao, "Klijent se nije odazvao", NotifikacijaTip.Opsta);

        public override async Task<TerminResponse> GetByIdAsync(int id)
        {
            var termin = await _dbContext.Termini
                .Include(t => t.Klijent)
                .Include(t => t.Frizer).ThenInclude(f => f.User)
                .Include(t => t.Usluga)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (termin == null)
                throw new KeyNotFoundException($"Termin with id {id} not found.");

            return new TerminResponse
            {
                Id = termin.Id,
                KlijentId = termin.KlijentId,
                KlijentImePrezime = termin.Klijent != null ? $"{termin.Klijent.FirstName} {termin.Klijent.LastName}".Trim() : null,
                FrizerId = termin.FrizerId,
                FrizerImePrezime = termin.Frizer?.User != null ? $"{termin.Frizer.User.FirstName} {termin.Frizer.User.LastName}".Trim() : null,
                UslugaId = termin.UslugaId,
                UslugaNaziv = termin.Usluga?.Naziv,
                DatumVrijeme = termin.DatumVrijeme,
                TrajanjeMinuta = termin.TrajanjeMinuta,
                Cijena = termin.Cijena,
                Status = termin.Status.ToString(),
                Napomena = termin.Napomena,
                CreatedAt = termin.CreatedAt,
                UpdatedAt = termin.UpdatedAt
            };
        }

        public async Task<List<string>> GetAllowedActionsAsync(int id)
        {
            var termin = await _dbContext.Termini.FindAsync(id);
            if (termin == null)
                throw new KeyNotFoundException($"Termin with id {id} not found.");

            return DozvoljeniPrelazi[termin.Status].Select(s => s.ToString()).ToList();
        }

        private async Task<TerminResponse> PromijeniStatusAsync(int id, TerminStatus noviStatus, string poruka, NotifikacijaTip tip)
        {
            var termin = await _dbContext.Termini.FindAsync(id);
            if (termin == null)
                throw new KeyNotFoundException($"Termin with id {id} not found.");

            if (!DozvoljeniPrelazi[termin.Status].Contains(noviStatus))
                throw new ClinetException($"Prelaz iz statusa '{termin.Status}' u '{noviStatus}' nije dozvoljen.");

            termin.Status = noviStatus;
            termin.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            await KreirajNotifikacijuAsync(termin, poruka, poruka, tip);

            return await GetByIdAsync(id);
        }

        private async Task KreirajNotifikacijuAsync(Termin termin, string naslov, string poruka, NotifikacijaTip tip)
        {
            var notifikacija = new Notifikacija
            {
                KorisnikId = termin.KlijentId,
                Naslov = naslov,
                Poruka = poruka,
                Tip = tip,
                Procitano = false,
                CreatedAt = DateTime.UtcNow,
                TerminId = termin.Id
            };

            _dbContext.Notifikacije.Add(notifikacija);
            await _dbContext.SaveChangesAsync();

            // Objavi u RabbitMQ da eCommerce.Subscriber (worker) posalje email obavjestenje.
            // Ovo je "best effort" - ako RabbitMQ nije pokrenut, termin i dalje ostaje uspjesno zakazan/promijenjen.
            var klijent = await _dbContext.Users.FindAsync(termin.KlijentId);
            if (klijent != null && !string.IsNullOrWhiteSpace(klijent.Email))
            {
                _rabbitMqPublisher.PublishNotifikacija(klijent.Email, naslov, poruka);
            }

            // Push uzivo (SignalR) - ako korisnik ima otvorenu desktop/mobilnu app, odmah vidi obavjestenje.
            await _realtimeNotifier.ObavijestiKorisnikaAsync(termin.KlijentId, new
            {
                notifikacija.Id,
                notifikacija.Naslov,
                notifikacija.Poruka,
                Tip = notifikacija.Tip.ToString(),
                notifikacija.Procitano,
                notifikacija.CreatedAt,
                notifikacija.TerminId
            });
        }
    }
}
