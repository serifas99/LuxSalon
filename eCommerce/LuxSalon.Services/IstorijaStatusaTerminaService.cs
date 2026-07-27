using LuxSalon.Model.Responses;
using LuxSalon.Model.SearchObjects;
using LuxSalon.Services.Database;
using Microsoft.EntityFrameworkCore;

namespace LuxSalon.Services
{
    /// <summary>
    /// Samo za citanje - audit trag se popunjava iskljucivo iz TerminService, nikad direktno preko API-ja.
    /// </summary>
    public class IstorijaStatusaTerminaService : BaseReadService<IstorijaStatusaTermina, IstorijaStatusaTerminaResponse, IstorijaStatusaTerminaSearchObject>, IIstorijaStatusaTerminaService
    {
        public IstorijaStatusaTerminaService(MapsterMapper.IMapper mapper, ECommerceDbContext dbContext) : base(mapper, dbContext)
        {
        }

        protected override IEnumerable<IstorijaStatusaTermina> ApplyFilters(IEnumerable<IstorijaStatusaTermina> query, IstorijaStatusaTerminaSearchObject? search)
        {
            if (search != null && search.TerminId.HasValue)
                query = query.Where(h => h.TerminId == search.TerminId.Value);

            return query.OrderByDescending(h => h.CreatedAt);
        }

        protected override async Task<IQueryable<IstorijaStatusaTermina>> IncludeRelatedEntitiesAsync(IstorijaStatusaTerminaSearchObject? search, IQueryable<IstorijaStatusaTermina> query = null)
        {
            query = query.Include(h => h.PromijenioKorisnik);
            return await Task.FromResult(query);
        }
    }
}
