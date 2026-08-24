using System.Globalization;
using LuxSalon.Model.Exceptions;
using LuxSalon.Model.Requests;
using LuxSalon.Model.Responses;
using LuxSalon.Model.SearchObjects;
using LuxSalon.Services.Database;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace LuxSalon.Services
{
    /// <summary>
    /// Radno vrijeme frizera po danu u sedmici. PocetakRada/KrajRada se u DTO-ovima predstavljaju
    /// kao "HH:mm" string (System.Text.Json ne podrzava TimeSpan direktno), a ovdje se konvertuju
    /// u/iz TimeSpan za bazu.
    /// </summary>
    public class RadnoVrijemeService : BaseCRUDService<RadnoVrijeme, RadnoVrijemeResponse, RadnoVrijemeSearchObject, RadnoVrijemeInsertRequest, RadnoVrijemeUpdateRequest>, IRadnoVrijemeService
    {
        public RadnoVrijemeService(ECommerceDbContext dbContext, MapsterMapper.IMapper mapper, IValidator<RadnoVrijemeInsertRequest> insertValidator, IValidator<RadnoVrijemeUpdateRequest> updateValidator) : base(dbContext, mapper, insertValidator, updateValidator)
        {
        }

        protected override IEnumerable<RadnoVrijeme> ApplyFilters(IEnumerable<RadnoVrijeme> query, RadnoVrijemeSearchObject? search)
        {
            if (search != null && search.FrizerId.HasValue)
                query = query.Where(r => r.FrizerId == search.FrizerId.Value);

            return query.OrderBy(r => r.FrizerId).ThenBy(r => r.DanUSedmici);
        }

        protected override async Task<IQueryable<RadnoVrijeme>> IncludeRelatedEntitiesAsync(RadnoVrijemeSearchObject? search, IQueryable<RadnoVrijeme> query = null)
        {
            query = query.Include(r => r.Frizer).ThenInclude(f => f.User);
            return await Task.FromResult(query);
        }

        public override async Task<RadnoVrijemeResponse> GetByIdAsync(int id)
        {
            var entity = await _dbContext.RadnaVremena
                .Include(r => r.Frizer).ThenInclude(f => f.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (entity == null)
                throw new KeyNotFoundException($"RadnoVrijeme with id {id} not found.");

            return MapToResponse(entity);
        }

        public override async Task<RadnoVrijemeResponse> InsertAsync(RadnoVrijemeInsertRequest request)
        {
            var validationResult = await _insertValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => _mapper.Map<FluentValidation.Results.ValidationFailure>(e));
                throw new FluentValidation.ValidationException(errors);
            }

            var frizer = await _dbContext.Frizeri.FindAsync(request.FrizerId);
            if (frizer == null)
                throw new ClientException("Odabrani frizer ne postoji.");

            var dan = (DayOfWeek)request.DanUSedmici;

            var postoji = await _dbContext.RadnaVremena.AnyAsync(r => r.FrizerId == request.FrizerId && r.DanUSedmici == dan);
            if (postoji)
                throw new ClientException("Radno vrijeme za taj dan je vec definisano za ovog frizera. Koristite izmjenu umjesto dodavanja.");

            var entity = new RadnoVrijeme
            {
                FrizerId = request.FrizerId,
                DanUSedmici = dan,
                PocetakRada = ParsirajVrijeme(request.PocetakRada),
                KrajRada = ParsirajVrijeme(request.KrajRada),
                NeRadi = request.NeRadi
            };

            _dbContext.RadnaVremena.Add(entity);
            await _dbContext.SaveChangesAsync();

            return await GetByIdAsync(entity.Id);
        }

        public override async Task<RadnoVrijemeResponse> UpdateAsync(int id, RadnoVrijemeUpdateRequest request)
        {
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => _mapper.Map<FluentValidation.Results.ValidationFailure>(e));
                throw new FluentValidation.ValidationException(errors);
            }

            var entity = await _dbContext.RadnaVremena.FindAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"RadnoVrijeme with id {id} not found.");

            entity.PocetakRada = ParsirajVrijeme(request.PocetakRada);
            entity.KrajRada = ParsirajVrijeme(request.KrajRada);
            entity.NeRadi = request.NeRadi;

            await _dbContext.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        private static TimeSpan ParsirajVrijeme(string vrijeme)
        {
            return TimeSpan.ParseExact(vrijeme, "hh\\:mm", CultureInfo.InvariantCulture);
        }

        private static RadnoVrijemeResponse MapToResponse(RadnoVrijeme entity)
        {
            return new RadnoVrijemeResponse
            {
                Id = entity.Id,
                FrizerId = entity.FrizerId,
                FrizerImePrezime = entity.Frizer?.User != null ? $"{entity.Frizer.User.FirstName} {entity.Frizer.User.LastName}".Trim() : null,
                DanUSedmici = (int)entity.DanUSedmici,
                DanUSedmiceNaziv = NazivDana(entity.DanUSedmici),
                PocetakRada = entity.PocetakRada.ToString(@"hh\:mm"),
                KrajRada = entity.KrajRada.ToString(@"hh\:mm"),
                NeRadi = entity.NeRadi
            };
        }

        private static string NazivDana(DayOfWeek dan) => dan switch
        {
            DayOfWeek.Monday => "Ponedjeljak",
            DayOfWeek.Tuesday => "Utorak",
            DayOfWeek.Wednesday => "Srijeda",
            DayOfWeek.Thursday => "Cetvrtak",
            DayOfWeek.Friday => "Petak",
            DayOfWeek.Saturday => "Subota",
            DayOfWeek.Sunday => "Nedjelja",
            _ => dan.ToString()
        };
    }
}
