using LuxSalon.Model.Requests;
using LuxSalon.Model.Responses;
using LuxSalon.Model.SearchObjects;
using LuxSalon.Services.Database;
using FluentValidation;

namespace LuxSalon.Services
{
    public class ObavijestService : BaseCRUDService<Obavijest, ObavijestResponse, ObavijestSearchObject, ObavijestInsertRequest, ObavijestUpdateRequest>, IObavijestService
    {
        public ObavijestService(ECommerceDbContext dbContext, MapsterMapper.IMapper mapper, IValidator<ObavijestInsertRequest> insertValidator, IValidator<ObavijestUpdateRequest> updateValidator) : base(dbContext, mapper, insertValidator, updateValidator)
        {
        }

        protected override IEnumerable<Obavijest> ApplyFilters(IEnumerable<Obavijest> query, ObavijestSearchObject? search)
        {
            if (search != null && search.IsActive.HasValue)
                query = query.Where(o => o.IsActive == search.IsActive.Value);

            return query.OrderByDescending(o => o.CreatedAt);
        }
    }
}
