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
    /// Klijent ocjenjuje frizera nakon odradjenog termina. KlijentId se ne prima od klijenta -
    /// uzima se iz JWT tokena (IAuthenticatedUserAccessor), a FrizerId se izvodi iz samog termina,
    /// tako da korisnik ne moze ocijeniti u tudje ime niti izmisliti FrizerId.
    /// </summary>
    public class FrizerOcjenaService : BaseCRUDService<FrizerOcjena, FrizerOcjenaResponse, FrizerOcjenaSearchObject, FrizerOcjenaInsertRequest, FrizerOcjenaUpdateRequest>, IFrizerOcjenaService
    {
        private readonly IAuthenticatedUserAccessor _userAccessor;

        public FrizerOcjenaService(ECommerceDbContext dbContext, MapsterMapper.IMapper mapper, IValidator<FrizerOcjenaInsertRequest> insertValidator, IValidator<FrizerOcjenaUpdateRequest> updateValidator, IAuthenticatedUserAccessor userAccessor) : base(dbContext, mapper, insertValidator, updateValidator)
        {
            _userAccessor = userAccessor;
        }

        protected override IEnumerable<FrizerOcjena> ApplyFilters(IEnumerable<FrizerOcjena> query, FrizerOcjenaSearchObject? search)
        {
            if (search != null)
            {
                if (search.FrizerId.HasValue)
                    query = query.Where(o => o.FrizerId == search.FrizerId.Value);

                if (search.KlijentId.HasValue)
                    query = query.Where(o => o.KlijentId == search.KlijentId.Value);
            }

            return query.OrderByDescending(o => o.CreatedAt);
        }

        protected override async Task<IQueryable<FrizerOcjena>> IncludeRelatedEntitiesAsync(FrizerOcjenaSearchObject? search, IQueryable<FrizerOcjena> query = null)
        {
            query = query.Include(o => o.Klijent).Include(o => o.Frizer).ThenInclude(f => f.User);
            return await Task.FromResult(query);
        }

        // Bazni GetByIdAsync koristi Find() bez Include-a, pa ne bi popunio KlijentImePrezime/FrizerImePrezime.
        public override async Task<FrizerOcjenaResponse> GetByIdAsync(int id)
        {
            var ocjena = await _dbContext.FrizerOcjene
                .Include(o => o.Klijent)
                .Include(o => o.Frizer).ThenInclude(f => f.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ocjena == null)
                throw new KeyNotFoundException($"FrizerOcjena with id {id} not found.");

            return _mapper.Map<FrizerOcjenaResponse>(ocjena);
        }

        public override async Task<FrizerOcjenaResponse> InsertAsync(FrizerOcjenaInsertRequest request)
        {
            var validationResult = await _insertValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => _mapper.Map<FluentValidation.Results.ValidationFailure>(e));
                throw new FluentValidation.ValidationException(errors);
            }

            var klijentId = _userAccessor.GetUserId();
            if (klijentId == null)
                throw new ClientException("Korisnik nije prijavljen.");

            var termin = await _dbContext.Termini.FindAsync(request.TerminId);
            if (termin == null)
                throw new KeyNotFoundException($"Termin with id {request.TerminId} not found.");

            if (termin.KlijentId != klijentId.Value)
                throw new ClientException("Mozete ocijeniti samo svoj termin.");

            if (termin.Status != TerminStatus.Odradjen)
                throw new ClientException("Frizera je moguce ocijeniti tek nakon odradjenog termina.");

            var postoji = await _dbContext.FrizerOcjene.AnyAsync(o => o.TerminId == request.TerminId);
            if (postoji)
                throw new ClientException("Ovaj termin je vec ocijenjen.");

            var ocjena = new FrizerOcjena
            {
                TerminId = request.TerminId,
                KlijentId = klijentId.Value,
                FrizerId = termin.FrizerId,
                Ocjena = request.Ocjena,
                Komentar = request.Komentar,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.FrizerOcjene.Add(ocjena);
            await _dbContext.SaveChangesAsync();

            return await GetByIdAsync(ocjena.Id);
        }

        public override async Task<FrizerOcjenaResponse> UpdateAsync(int id, FrizerOcjenaUpdateRequest request)
        {
            var entity = await _dbContext.FrizerOcjene.FindAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"FrizerOcjena with id {id} not found.");

            if (!_userAccessor.IsInRole("Admin") && entity.KlijentId != _userAccessor.GetUserId())
                throw new ClientException("Mozete urediti samo svoju ocjenu.");

            return await base.UpdateAsync(id, request);
        }

        public override async Task DeleteAsync(int id)
        {
            var entity = await _dbContext.FrizerOcjene.FindAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"FrizerOcjena with id {id} not found.");

            if (!_userAccessor.IsInRole("Admin") && entity.KlijentId != _userAccessor.GetUserId())
                throw new ClientException("Mozete obrisati samo svoju ocjenu.");

            await base.DeleteAsync(id);
        }

        public async Task<double> ProsjecnaOcjenaAsync(int frizerId)
        {
            var ocjene = await _dbContext.FrizerOcjene
                .Where(o => o.FrizerId == frizerId)
                .Select(o => o.Ocjena)
                .ToListAsync();

            return ocjene.Count == 0 ? 0 : ocjene.Average();
        }
    }
}
