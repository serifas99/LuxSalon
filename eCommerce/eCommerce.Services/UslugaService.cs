using eCommerce.Model.Requests;
using eCommerce.Model.Responses;
using eCommerce.Model.SearchObjects;
using eCommerce.Services.Database;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Services
{
    public class UslugaService : BaseCRUDService<Usluga, UslugaResponse, UslugaSearchObject, UslugaInsertRequest, UslugaUpdateRequest>, IUslugaService
    {
        public UslugaService(ECommerceDbContext dbContext, MapsterMapper.IMapper mapper, IValidator<UslugaInsertRequest> insertValidator, IValidator<UslugaUpdateRequest> updateValidator) : base(dbContext, mapper, insertValidator, updateValidator)
        {
        }

        protected override async Task<IQueryable<Usluga>> IncludeRelatedEntitiesAsync(UslugaSearchObject? search, IQueryable<Usluga> query = null)
        {
            query = query.Include(u => u.UslugaKategorija);
            return await Task.FromResult(query);
        }

        protected override IEnumerable<Usluga> ApplyFilters(IEnumerable<Usluga> query, UslugaSearchObject? search)
        {
            if (search != null)
            {
                if (!string.IsNullOrWhiteSpace(search.Naziv))
                {
                    query = query.Where(u => u.Naziv.Contains(search.Naziv, StringComparison.OrdinalIgnoreCase));
                }

                if (search.UslugaKategorijaId.HasValue)
                {
                    query = query.Where(u => u.UslugaKategorijaId == search.UslugaKategorijaId.Value);
                }

                if (search.IsActive.HasValue)
                {
                    query = query.Where(u => u.IsActive == search.IsActive.Value);
                }

                if (!string.IsNullOrWhiteSpace(search.Tag))
                {
                    query = query.Where(u => u.Tagovi.Contains(search.Tag, StringComparison.OrdinalIgnoreCase));
                }
            }

            return query;
        }
    }
}
