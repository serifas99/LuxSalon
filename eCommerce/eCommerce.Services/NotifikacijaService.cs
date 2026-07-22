using eCommerce.Model.Requests;
using eCommerce.Model.Responses;
using eCommerce.Model.SearchObjects;
using eCommerce.Services.Database;
using FluentValidation;

namespace eCommerce.Services
{
    public class NotifikacijaService : BaseCRUDService<Notifikacija, NotifikacijaResponse, NotifikacijaSearchObject, NotifikacijaInsertRequest, NotifikacijaUpdateRequest>, INotifikacijaService
    {
        public NotifikacijaService(ECommerceDbContext dbContext, MapsterMapper.IMapper mapper, IValidator<NotifikacijaInsertRequest> insertValidator, IValidator<NotifikacijaUpdateRequest> updateValidator) : base(dbContext, mapper, insertValidator, updateValidator)
        {
        }

        protected override IEnumerable<Notifikacija> ApplyFilters(IEnumerable<Notifikacija> query, NotifikacijaSearchObject? search)
        {
            if (search != null)
            {
                if (search.KorisnikId.HasValue)
                    query = query.Where(n => n.KorisnikId == search.KorisnikId.Value);

                if (search.Procitano.HasValue)
                    query = query.Where(n => n.Procitano == search.Procitano.Value);
            }

            return query.OrderByDescending(n => n.CreatedAt);
        }

        public async Task<NotifikacijaResponse> OznaciProcitanoAsync(int id)
        {
            var notifikacija = await _dbContext.Notifikacije.FindAsync(id);
            if (notifikacija == null)
                throw new KeyNotFoundException($"Notifikacija with id {id} not found.");

            notifikacija.Procitano = true;
            await _dbContext.SaveChangesAsync();

            return await GetByIdAsync(id);
        }
    }
}
