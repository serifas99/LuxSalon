using eCommerce.Model.Requests;
using eCommerce.Model.Responses;
using eCommerce.Model.SearchObjects;
using eCommerce.Services.Database;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Services
{
    public class FrizerService : BaseCRUDService<Frizer, FrizerResponse, FrizerSearchObject, FrizerInsertRequest, FrizerUpdateRequest>, IFrizerService
    {
        public FrizerService(ECommerceDbContext dbContext, MapsterMapper.IMapper mapper, IValidator<FrizerInsertRequest> insertValidator, IValidator<FrizerUpdateRequest> updateValidator) : base(dbContext, mapper, insertValidator, updateValidator)
        {
        }

        protected override async Task<IQueryable<Frizer>> IncludeRelatedEntitiesAsync(FrizerSearchObject? search, IQueryable<Frizer> query = null)
        {
            query = query.Include(f => f.User).Include(f => f.FrizerUsluge);
            return await Task.FromResult(query);
        }

        protected override IEnumerable<Frizer> ApplyFilters(IEnumerable<Frizer> query, FrizerSearchObject? search)
        {
            if (search != null)
            {
                if (!string.IsNullOrWhiteSpace(search.Ime))
                {
                    query = query.Where(f => (f.User.FirstName + " " + f.User.LastName).Contains(search.Ime, StringComparison.OrdinalIgnoreCase));
                }

                if (search.UslugaId.HasValue)
                {
                    query = query.Where(f => f.FrizerUsluge.Any(fu => fu.UslugaId == search.UslugaId.Value));
                }

                if (search.IsActive.HasValue)
                {
                    query = query.Where(f => f.IsActive == search.IsActive.Value);
                }
            }

            return query;
        }

        public override async Task<FrizerResponse> InsertAsync(FrizerInsertRequest request)
        {
            var validationResult = await _insertValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => _mapper.Map<ValidationFailure>(e));
                throw new FluentValidation.ValidationException(errors);
            }

            var frizer = new Frizer
            {
                UserId = request.UserId,
                Biografija = request.Biografija,
                Specijalizacija = request.Specijalizacija,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Frizeri.Add(frizer);
            await _dbContext.SaveChangesAsync();

            foreach (var uslugaId in request.UslugaIds.Distinct())
            {
                _dbContext.FrizerUsluge.Add(new FrizerUsluga { FrizerId = frizer.Id, UslugaId = uslugaId });
            }
            await _dbContext.SaveChangesAsync();

            return await GetByIdAsync(frizer.Id);
        }

        public override async Task<FrizerResponse> UpdateAsync(int id, FrizerUpdateRequest request)
        {
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => _mapper.Map<ValidationFailure>(e));
                throw new FluentValidation.ValidationException(errors);
            }

            var frizer = await _dbContext.Frizeri.FindAsync(id);
            if (frizer == null)
                throw new KeyNotFoundException($"Frizer with id {id} not found.");

            frizer.Biografija = request.Biografija;
            frizer.Specijalizacija = request.Specijalizacija;
            frizer.IsActive = request.IsActive;

            var postojece = _dbContext.FrizerUsluge.Where(fu => fu.FrizerId == id);
            _dbContext.FrizerUsluge.RemoveRange(postojece);

            foreach (var uslugaId in request.UslugaIds.Distinct())
            {
                _dbContext.FrizerUsluge.Add(new FrizerUsluga { FrizerId = id, UslugaId = uslugaId });
            }

            await _dbContext.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public override async Task<FrizerResponse> GetByIdAsync(int id)
        {
            var frizer = await _dbContext.Frizeri
                .Include(f => f.User)
                .Include(f => f.FrizerUsluge)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (frizer == null)
                throw new KeyNotFoundException($"Frizer with id {id} not found.");

            return MapToResponse(frizer);
        }

        private FrizerResponse MapToResponse(Frizer frizer)
        {
            return new FrizerResponse
            {
                Id = frizer.Id,
                UserId = frizer.UserId,
                ImePrezime = frizer.User != null ? $"{frizer.User.FirstName} {frizer.User.LastName}".Trim() : string.Empty,
                Email = frizer.User?.Email,
                Biografija = frizer.Biografija,
                Specijalizacija = frizer.Specijalizacija,
                IsActive = frizer.IsActive,
                CreatedAt = frizer.CreatedAt,
                UslugaIds = frizer.FrizerUsluge?.Select(fu => fu.UslugaId).ToList() ?? new List<int>()
            };
        }
    }
}
