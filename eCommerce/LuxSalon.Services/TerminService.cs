using LuxSalon.Common.Services.Messaging;
using LuxSalon.Common.Services.Realtime;
using LuxSalon.Model.Exceptions;
using LuxSalon.Model.Requests;
using LuxSalon.Model.Responses;
using LuxSalon.Model.SearchObjects;
using LuxSalon.Services.Database;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace LuxSalon.Services
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
        private readonly IAuthenticatedUserAccessor _userAccessor;

        public TerminService(ECommerceDbContext dbContext, MapsterMapper.IMapper mapper, IValidator<TerminInsertRequest> insertValidator, IValidator<TerminUpdateRequest> updateValidator, IRabbitMqPublisher rabbitMqPublisher, IRealtimeNotifier realtimeNotifier, IAuthenticatedUserAccessor userAccessor) : base(dbContext, mapper, insertValidator, updateValidator)
        {
            _rabbitMqPublisher = rabbitMqPublisher;
            _realtimeNotifier = realtimeNotifier;
            _userAccessor = userAccessor;
        }

        protected override async Task<IQueryable<Termin>> IncludeRelatedEntitiesAsync(TerminSearchObject? search, IQueryable<Termin> query = null)
        {
            query = query.Include(t => t.Klijent).Include(t => t.Frizer).ThenInclude(f => f.User).Include(t => t.Usluga).Include(t => t.Placanje);
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
                .Include(t => t.Placanje)
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
                PlacanjeStatus = termin.Placanje?.Status.ToString(),
                PlacanjeId = termin.Placanje?.Id,
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

        private const int KorakSlotaMinuta = 30;
        private static readonly TerminStatus[] AktivniStatusiZaZauzetost = { TerminStatus.Zakazan, TerminStatus.Potvrdjen };

        public async Task<List<DostupnostDanaResponse>> DostupnostAsync(int frizerId, int uslugaId, int godina, int mjesec)
        {
            var usluga = await _dbContext.Usluge.FindAsync(uslugaId);
            if (usluga == null)
                throw new ClinetException("Odabrana usluga ne postoji.");

            var radnaVremena = await _dbContext.RadnaVremena
                .Where(r => r.FrizerId == frizerId)
                .ToListAsync();

            var pocetakMjeseca = new DateTime(godina, mjesec, 1);
            var krajMjeseca = pocetakMjeseca.AddMonths(1);

            var terminiUMjesecu = await _dbContext.Termini
                .Where(t => t.FrizerId == frizerId &&
                            AktivniStatusiZaZauzetost.Contains(t.Status) &&
                            t.DatumVrijeme >= pocetakMjeseca && t.DatumVrijeme < krajMjeseca)
                .Select(t => new { t.DatumVrijeme, t.TrajanjeMinuta })
                .ToListAsync();

            var rezultat = new List<DostupnostDanaResponse>();
            int brojDanaUMjesecu = DateTime.DaysInMonth(godina, mjesec);

            for (int dan = 1; dan <= brojDanaUMjesecu; dan++)
            {
                var datum = new DateTime(godina, mjesec, dan);
                var radnoVrijeme = radnaVremena.FirstOrDefault(r => r.DanUSedmici == datum.DayOfWeek);

                if (radnoVrijeme == null || radnoVrijeme.NeRadi)
                {
                    rezultat.Add(new DostupnostDanaResponse { Datum = datum, Radi = false, Slobodno = false, BrojSlobodnihSlotova = 0 });
                    continue;
                }

                var terminiTogDana = terminiUMjesecu
                    .Where(t => t.DatumVrijeme.Date == datum.Date)
                    .Select(t => (t.DatumVrijeme, t.TrajanjeMinuta))
                    .ToList();

                int brojSlobodnih = IzracunajSlobodneSlotove(datum, radnoVrijeme.PocetakRada, radnoVrijeme.KrajRada, usluga.TrajanjeMinuta, terminiTogDana).Count;

                rezultat.Add(new DostupnostDanaResponse
                {
                    Datum = datum,
                    Radi = true,
                    Slobodno = brojSlobodnih > 0,
                    BrojSlobodnihSlotova = brojSlobodnih
                });
            }

            return rezultat;
        }

        public async Task<List<string>> DostupniSlotoviAsync(int frizerId, int uslugaId, DateTime datum)
        {
            var usluga = await _dbContext.Usluge.FindAsync(uslugaId);
            if (usluga == null)
                throw new ClinetException("Odabrana usluga ne postoji.");

            var radnoVrijeme = await _dbContext.RadnaVremena
                .FirstOrDefaultAsync(r => r.FrizerId == frizerId && r.DanUSedmici == datum.DayOfWeek);

            if (radnoVrijeme == null || radnoVrijeme.NeRadi)
                return new List<string>();

            var terminiTogDana = await _dbContext.Termini
                .Where(t => t.FrizerId == frizerId &&
                            AktivniStatusiZaZauzetost.Contains(t.Status) &&
                            t.DatumVrijeme.Date == datum.Date)
                .Select(t => new { t.DatumVrijeme, t.TrajanjeMinuta })
                .ToListAsync();

            var slobodniSlotovi = IzracunajSlobodneSlotove(
                datum, radnoVrijeme.PocetakRada, radnoVrijeme.KrajRada, usluga.TrajanjeMinuta,
                terminiTogDana.Select(t => (t.DatumVrijeme, t.TrajanjeMinuta)).ToList());

            return slobodniSlotovi.Select(s => s.ToString("HH:mm")).ToList();
        }

        // Racuna sve slobodne pocetne termine (u koraku od KorakSlotaMinuta) unutar radnog vremena za
        // zadani dan, izbjegavajuci preklapanje sa vec zauzetim (ne-otkazanim) terminima tog frizera
        // i izbjegavajuci termine u proslosti.
        private static List<DateTime> IzracunajSlobodneSlotove(DateTime datum, TimeSpan pocetakRada, TimeSpan krajRada, int trajanjeMinuta, List<(DateTime DatumVrijeme, int TrajanjeMinuta)> zauzetiTermini)
        {
            var slobodni = new List<DateTime>();
            var pocetakDana = datum.Date + pocetakRada;
            var krajDana = datum.Date + krajRada;

            for (var slot = pocetakDana; slot.AddMinutes(trajanjeMinuta) <= krajDana; slot = slot.AddMinutes(KorakSlotaMinuta))
            {
                if (slot < DateTime.Now)
                    continue;

                var slotKraj = slot.AddMinutes(trajanjeMinuta);
                bool preklapa = zauzetiTermini.Any(z =>
                    slot < z.DatumVrijeme.AddMinutes(z.TrajanjeMinuta) &&
                    slotKraj > z.DatumVrijeme);

                if (!preklapa)
                    slobodni.Add(slot);
            }

            return slobodni;
        }

        // Klijent smije samo otkazati SVOJ termin - sve ostale promjene statusa (potvrda, odradjen,
        // nije se odazvao) su akcije osoblja (Admin/Frizer). Bez ove provjere bi bilo koji prijavljeni
        // klijent mogao mijenjati status tudjeg termina pozivom akcije direktno preko API-ja.
        private void ProvjeriOvlascenjeZaPromjenuStatusa(Termin termin, TerminStatus noviStatus)
        {
            var jeOsoblje = _userAccessor.IsInRole("Admin") || _userAccessor.IsInRole("Frizer");
            if (jeOsoblje)
                return;

            if (noviStatus != TerminStatus.Otkazan)
                throw new ClinetException("Samo osoblje (frizer/admin) moze promijeniti ovaj status termina.");

            var korisnikId = _userAccessor.GetUserId();
            if (korisnikId == null || termin.KlijentId != korisnikId.Value)
                throw new ClinetException("Mozete otkazati samo svoj termin.");
        }

        private async Task<TerminResponse> PromijeniStatusAsync(int id, TerminStatus noviStatus, string poruka, NotifikacijaTip tip)
        {
            var termin = await _dbContext.Termini.FindAsync(id);
            if (termin == null)
                throw new KeyNotFoundException($"Termin with id {id} not found.");

            ProvjeriOvlascenjeZaPromjenuStatusa(termin, noviStatus);

            if (!DozvoljeniPrelazi[termin.Status].Contains(noviStatus))
                throw new ClinetException($"Prelaz iz statusa '{termin.Status}' u '{noviStatus}' nije dozvoljen.");

            var prethodniStatus = termin.Status;
            termin.Status = noviStatus;
            termin.UpdatedAt = DateTime.UtcNow;

            // Audit trag - ko je promijenio status i kada (zahtjev iz uputstva za seminarski rad).
            var korisnikId = _userAccessor.GetUserId() ?? termin.KlijentId;
            _dbContext.IstorijaStatusaTermina.Add(new IstorijaStatusaTermina
            {
                TerminId = termin.Id,
                PrethodniStatus = prethodniStatus,
                NoviStatus = noviStatus,
                PromijenioKorisnikId = korisnikId,
                Opis = poruka,
                CreatedAt = DateTime.UtcNow
            });

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

            // Objavi u RabbitMQ da LuxSalon.Subscriber (worker) posalje email obavjestenje.
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
