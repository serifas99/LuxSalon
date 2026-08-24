using LuxSalon.Model.Requests;
using LuxSalon.Model.Responses;
using LuxSalon.Model.SearchObjects;
using LuxSalon.Services.Database;
using FluentValidation;

namespace LuxSalon.Services
{
    public class NotifikacijaService : BaseCRUDService<Notifikacija, NotifikacijaResponse, NotifikacijaSearchObject, NotifikacijaInsertRequest, NotifikacijaUpdateRequest>, INotifikacijaService
    {
        private readonly IAuthenticatedUserAccessor _userAccessor;

        public NotifikacijaService(ECommerceDbContext dbContext, MapsterMapper.IMapper mapper, IValidator<NotifikacijaInsertRequest> insertValidator, IValidator<NotifikacijaUpdateRequest> updateValidator, IAuthenticatedUserAccessor userAccessor) : base(dbContext, mapper, insertValidator, updateValidator)
        {
            _userAccessor = userAccessor;
        }

        protected override IEnumerable<Notifikacija> ApplyFilters(IEnumerable<Notifikacija> query, NotifikacijaSearchObject? search)
        {
            // Klijent smije vidjeti samo svoje notifikacije - ne vjerujemo KorisnikId-u iz querya
            // (osim za Admina), da tudji korisnikId proslijedjen kroz URL ne bi otkrio tudje poruke.
            if (!_userAccessor.IsInRole("Admin"))
            {
                var korisnikId = _userAccessor.GetUserId();
                query = query.Where(n => n.KorisnikId == korisnikId);
            }
            else if (search?.KorisnikId != null)
            {
                query = query.Where(n => n.KorisnikId == search.KorisnikId.Value);
            }

            if (search?.Procitano != null)
                query = query.Where(n => n.Procitano == search.Procitano.Value);

            return query.OrderByDescending(n => n.CreatedAt);
        }

        public async Task<NotifikacijaResponse> OznaciProcitanoAsync(int id)
        {
            var notifikacija = await _dbContext.Notifikacije.FindAsync(id);
            if (notifikacija == null)
                throw new KeyNotFoundException($"Notifikacija with id {id} not found.");

            if (!_userAccessor.IsInRole("Admin") && notifikacija.KorisnikId != _userAccessor.GetUserId())
                throw new LuxSalon.Model.Exceptions.ClientException("Mozete oznaciti procitanim samo svoju notifikaciju.");

            notifikacija.Procitano = true;
            await _dbContext.SaveChangesAsync();

            return await GetByIdAsync(id);
        }
    }
}
