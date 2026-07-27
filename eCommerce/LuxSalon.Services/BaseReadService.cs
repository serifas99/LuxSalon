using LuxSalon.Model.Responses;
using LuxSalon.Model.SearchObjects;
using LuxSalon.Services.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;

namespace LuxSalon.Services
{
    public abstract class BaseReadService<TEntity, TResponse, TSearch> : IBaseReadService<TResponse, TSearch>
        where TEntity : class
        where TSearch : BaseSearchObject
    {
        // Server-side maksimalni limit za PageSize (uputa RS2: "PageSize mora imati definisan
        // maksimalni limit, npr. 100" - klijent moze traziti manje, ali nikad vise od ovoga,
        // bez obzira sta posalje u query stringu).
        protected const int MaxPageSize = 100;

        protected readonly MapsterMapper.IMapper _mapper;
        protected readonly ECommerceDbContext _dbContext;

        protected BaseReadService(MapsterMapper.IMapper mapper, ECommerceDbContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }


        /// <summary>
        /// Applies search filters to the query. Override in derived classes to implement specific filtering logic.
        /// </summary>
        protected abstract IEnumerable<TEntity> ApplyFilters(IEnumerable<TEntity> query, TSearch? search);

        public virtual async Task<PageResult<TResponse>> GetAllAsync(TSearch? search = null)
        {
            IEnumerable<TEntity> query = this._dbContext.Set<TEntity>();

            query = await IncludeRelatedEntitiesAsync(search, query.AsQueryable());
            query = ApplyFilters(query, search);

            int? totalCount = null;

            if (search.IncludeTotalCount ?? false)
            {
                totalCount = query.Count();
            }

            if (!string.IsNullOrWhiteSpace(search.SortBy))
            {
                //TODO: parametrize sortBy to prevent SQL injection
                query = query.AsQueryable().OrderBy(search.SortBy);
            }

            var effectivePageSize = Math.Min(search.PageSize ?? MaxPageSize, MaxPageSize);

            if (search.Page.HasValue)
            {
                query = query.Skip((search.Page.Value - 1) * effectivePageSize);
            }

            query = query.Take(effectivePageSize);

            var list = query.Select(item => _mapper.Map<TResponse>(item)).ToList();

            var pageResult = new PageResult<TResponse>
            {
                Items = list,
                TotalCount = totalCount
            };

            return await Task.FromResult(pageResult);
        }

        protected virtual async Task<IQueryable<TEntity>> IncludeRelatedEntitiesAsync(TSearch? search, IQueryable<TEntity> query = null)
        {
            // Override in derived classes to include related entities if necessary
            return query;
        }


        public virtual async Task<TResponse> GetByIdAsync(int id)
        {
            var entity = this._dbContext.Set<TEntity>().Find(id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"{typeof(TEntity).Name} with id {id} not found.");
            }

            return await Task.FromResult(_mapper.Map<TResponse>(entity));
        }
    }
}
