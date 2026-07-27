using LuxSalon.Model.Requests;
using LuxSalon.Model.Responses;
using LuxSalon.Model.SearchObjects;
using LuxSalon.Services.Database;
using FluentValidation;

namespace LuxSalon.Services
{
    public class UslugaKategorijaService : BaseCRUDService<UslugaKategorija, UslugaKategorijaResponse, UslugaKategorijaSearchObject, UslugaKategorijaInsertRequest, UslugaKategorijaUpdateRequest>, IUslugaKategorijaService
    {
        public UslugaKategorijaService(ECommerceDbContext dbContext, MapsterMapper.IMapper mapper, IValidator<UslugaKategorijaInsertRequest> insertValidator, IValidator<UslugaKategorijaUpdateRequest> updateValidator) : base(dbContext, mapper, insertValidator, updateValidator)
        {
        }

        protected override IEnumerable<UslugaKategorija> ApplyFilters(IEnumerable<UslugaKategorija> query, UslugaKategorijaSearchObject? search)
        {
            if (search != null)
            {
                if (!string.IsNullOrWhiteSpace(search.Naziv))
                {
                    query = query.Where(k => k.Naziv.Contains(search.Naziv, StringComparison.OrdinalIgnoreCase));
                }

                if (search.IsActive.HasValue)
                {
                    query = query.Where(k => k.IsActive == search.IsActive.Value);
                }
            }

            return query;
        }
    }
}
