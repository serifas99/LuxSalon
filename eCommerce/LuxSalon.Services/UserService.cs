using LuxSalon.Common.Services.CryptoService;
using LuxSalon.Model.Access;
using LuxSalon.Model.Exceptions;
using LuxSalon.Model.Requests;
using LuxSalon.Model.Responses;
using LuxSalon.Model.SearchObjects;
using LuxSalon.Services.Database;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LuxSalon.Services
{
    public class UserService : BaseCRUDService<User, UserResponse, UserSearch, UserInsertRequest, UserUpdateRequest>, IUserService
    {
        private readonly ICryptoService _cryptoService;
        private readonly IAuthenticatedUserAccessor _userAccessor;
        public UserService(ECommerceDbContext dbContext, MapsterMapper.IMapper mapper, IValidator<UserInsertRequest> insertValidator, IValidator<UserUpdateRequest> updateValidator, ICryptoService cryptoService, IAuthenticatedUserAccessor userAccessor)
            : base(dbContext, mapper, insertValidator, updateValidator)
        {
            _cryptoService = cryptoService;
            _userAccessor = userAccessor;
        }


        // Bez ovoga generickie GetAll ne ucitava UserRoles/Role iz baze (EF Core ne lazy-loada
        // navigacije), pa Role u UserResponse ostaje null bez obzira na Mapster konfiguraciju.
        protected override async Task<IQueryable<User>> IncludeRelatedEntitiesAsync(UserSearch? search, IQueryable<User> query = null)
        {
            query = query.Include(u => u.UserRoles).ThenInclude(ur => ur.Role);
            return await Task.FromResult(query);
        }

        protected override IEnumerable<User> ApplyFilters(IEnumerable<User> query, UserSearch? search)
        {
            if (search != null)
            {
                if (!string.IsNullOrWhiteSpace(search.Email))
                {
                    query = query.Where(u => u.Email.Contains(search.Email, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(search.Username))
                {
                    query = query.Where(u => u.Username.Contains(search.Username, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(search.Name))
                {
                    query = query.Where(u => u.FirstName.Contains(search.Name, StringComparison.OrdinalIgnoreCase)
                                          || u.LastName.Contains(search.Name, StringComparison.OrdinalIgnoreCase));
                }

                if (search.IsActive.HasValue)
                {
                    query = query.Where(u => u.IsActive == search.IsActive.Value);
                }
            }

            return query;
        }

        public async Task<PageResult<KlijentPregledResponse>> GetKlijentiAsync(UserSearch search)
        {
            var query = _dbContext.Users
                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Customer"));

            if (!string.IsNullOrWhiteSpace(search.Name))
            {
                query = query.Where(u => u.FirstName.Contains(search.Name, StringComparison.OrdinalIgnoreCase)
                                       || u.LastName.Contains(search.Name, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(search.Email))
            {
                query = query.Where(u => u.Email.Contains(search.Email, StringComparison.OrdinalIgnoreCase));
            }

            query = query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName);

            int? totalCount = null;
            if (search.IncludeTotalCount == true)
                totalCount = await query.CountAsync();

            if (search.Page.HasValue && search.PageSize.HasValue)
                query = query.Skip((search.Page.Value - 1) * search.PageSize.Value).Take(search.PageSize.Value);

            var klijenti = await query.ToListAsync();
            var klijentIds = klijenti.Select(k => k.Id).ToList();

            // Agregacija (broj termina, datum posljednjeg) - posebnim upitom nad Termini,
            // jer se ne moze uraditi generickim Mapster mapiranjem (izracunata polja).
            var terminStats = await _dbContext.Termini
                .Where(t => klijentIds.Contains(t.KlijentId))
                .GroupBy(t => t.KlijentId)
                .Select(g => new
                {
                    KlijentId = g.Key,
                    Broj = g.Count(),
                    Posljednji = g.Max(t => t.DatumVrijeme)
                })
                .ToDictionaryAsync(x => x.KlijentId);

            var items = klijenti.Select(k =>
            {
                terminStats.TryGetValue(k.Id, out var stats);
                return new KlijentPregledResponse
                {
                    Id = k.Id,
                    ImePrezime = $"{k.FirstName} {k.LastName}".Trim(),
                    Email = k.Email,
                    BrojZakazanihTermina = stats?.Broj ?? 0,
                    DatumPosljednjegTermina = stats?.Posljednji
                };
            }).ToList();

            return new PageResult<KlijentPregledResponse> { Items = items, TotalCount = totalCount };
        }

        protected override User MapInsertRequestToEntity(UserInsertRequest request)
        {
            ProvjeriProfilnuSliku(request.ProfileImageBase64);

            var entity = base.MapInsertRequestToEntity(request);

            // Handle password hashing for User entity
            var salt = _cryptoService.GenerateSlat();
            entity.PasswordSalt = salt;
            entity.PasswordHash = _cryptoService.GenerateHash(request.Password, salt);

            return entity;
        }

        private const int MaxProfilnaSlikaBajtova = 3 * 1024 * 1024; // 3 MB

        // Upute eksplicitno traze da svaki upload fajla validira stvarni tip sadrzaja preko
        // "magic bytes" potpisa, ne samo ekstenziju/Content-Type koje klijent (i napadac) moze
        // slobodno izmisliti. Slika se ovdje salje kao base64 string ugradjen u JSON (ne
        // multipart/form-data), ali isti princip i dalje vrijedi - provjeravamo stvarne bajtove
        // dekodirane slike, ne ono sto klijent tvrdi da je poslao.
        private static void ProvjeriProfilnuSliku(string? base64Slika)
        {
            if (string.IsNullOrWhiteSpace(base64Slika))
                return;

            byte[] bajtovi;
            try
            {
                bajtovi = Convert.FromBase64String(base64Slika);
            }
            catch (FormatException)
            {
                throw new ClinetException("Slika profila nije ispravno kodirana.");
            }

            if (bajtovi.Length == 0)
                throw new ClinetException("Slika profila je prazna.");

            if (bajtovi.Length > MaxProfilnaSlikaBajtova)
                throw new ClinetException("Slika profila ne smije biti veca od 3 MB.");

            bool jePng = bajtovi.Length >= 8 &&
                bajtovi[0] == 0x89 && bajtovi[1] == 0x50 && bajtovi[2] == 0x4E && bajtovi[3] == 0x47 &&
                bajtovi[4] == 0x0D && bajtovi[5] == 0x0A && bajtovi[6] == 0x1A && bajtovi[7] == 0x0A;

            bool jeJpeg = bajtovi.Length >= 3 &&
                bajtovi[0] == 0xFF && bajtovi[1] == 0xD8 && bajtovi[2] == 0xFF;

            if (!jePng && !jeJpeg)
                throw new ClinetException("Slika profila mora biti u JPEG ili PNG formatu.");
        }

        public override async Task<UserResponse> InsertAsync(UserInsertRequest request)
        {
            // let FluentValidation throw if the request isn't valid; the exception filter will
            // convert the resulting ValidationException into the standard error format.
            await _insertValidator.ValidateAndThrowAsync(request);

            // Check if email or username already exists
            if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email))
            {
                throw new InvalidOperationException($"Email '{request.Email}' is already in use.");
            }

            if (await _dbContext.Users.AnyAsync(u => u.Username == request.Username))
            {
                throw new InvalidOperationException($"Username '{request.Username}' is already in use.");
            }

            var entity = MapInsertRequestToEntity(request);
            entity.CreatedAt = DateTime.UtcNow;

            _dbContext.Users.Add(entity);
            await _dbContext.SaveChangesAsync();

            // Svaki novoregistrovan korisnik (npr. kroz mobilnu Register formu, ili kroz ovaj
            // isti endpoint sa desktopa) automatski dobija rolu "Customer" - bez ovoga korisnik
            // ne bi imao nijednu rolu, pa se ne bi pojavljivao na "Klijenti" ekranu (koji filtrira
            // po roli) niti bi mu login token nosio ispravnu rolu. Admin/Frizer role se dodjeljuju
            // posebno (seed podaci, ili rucno preko baze/Scalar-a), ne kroz javnu registraciju.
            var customerRoleId = await _dbContext.Roles
                .Where(r => r.Name == "Customer")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (customerRoleId != 0)
            {
                _dbContext.UserRoles.Add(new UserRole
                {
                    UserId = entity.Id,
                    RoleId = customerRoleId,
                    DateAssigned = DateTime.UtcNow
                });
                await _dbContext.SaveChangesAsync();
            }

            return _mapper.Map<UserResponse>(entity);
        }


        public override async Task<UserResponse> UpdateAsync(int id, UserUpdateRequest request)
        {
            // Korisnik smije urediti samo svoj nalog, osim ako je Admin (upravlja svim nalozima
            // sa desktopa) - bez ovoga bi bilo koji prijavljeni klijent mogao pozivom PUT /Users/{tudjiId}
            // izmijeniti podatke (pa i deaktivirati) bilo koji drugi nalog, ukljucujuci Admin nalog.
            if (!_userAccessor.IsInRole("Admin"))
            {
                var korisnikId = _userAccessor.GetUserId();
                if (korisnikId == null || korisnikId.Value != id)
                    throw new ClinetException("Mozete urediti samo svoj nalog.");
            }

            await _updateValidator.ValidateAndThrowAsync(request);
            ProvjeriProfilnuSliku(request.ProfileImageBase64);

            var entity = await _dbContext.Users.FindAsync(id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"User with id {id} not found.");
            }

            // Check if email or username already exists
            if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email && u.Id != id))
            {
                throw new InvalidOperationException($"Email '{request.Email}' is already in use.");
            }

            if (await _dbContext.Users.AnyAsync(u => u.Username == request.Username && u.Id != id))
            {
                throw new InvalidOperationException($"Username '{request.Username}' is already in use.");
            }

            MapUpdateRequestToEntity(request, entity);

            _dbContext.Users.Update(entity);
            await _dbContext.SaveChangesAsync();

            return _mapper.Map<UserResponse>(entity);
        }

        public override async Task DeleteAsync(int id)
        {
            // Brisanje naloga je iskljucivo administratorska akcija.
            if (!_userAccessor.IsInRole("Admin"))
                throw new ClinetException("Samo administrator moze obrisati korisnicki nalog.");

            var entity = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"User with id {id} not found.");
            }

            _dbContext.Users.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<UserSensitveResponse?> GetByUsernameAsync(string username)
        {
            var user = await _dbContext.Users
                .AsNoTracking()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == username);

            UserSensitveResponse? response = null;

            if (user != null)
            {
                response = _mapper.Map<UserSensitveResponse>(user);
                response.Role = user.UserRoles.FirstOrDefault()?.Role.Name;
            }

            return response;
        }

        public async Task<UserResponse?> GetWithRoleByIdAsync(int id)
        {
            var user = await _dbContext.Users
               .AsNoTracking()
               .Include(u => u.UserRoles)
               .ThenInclude(ur => ur.Role)
               .FirstOrDefaultAsync(u => u.Id == id);

            UserResponse? response = null;

            if (user != null)
            {
                response = _mapper.Map<UserResponse>(user);
                response.Role = user.UserRoles.First().Role.Name;
            }

            return response;
        }

        public async Task ChangePasswordAsync(UserPasswordChangeRequest request)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == request.Id);

            if (user == null)
                throw new Exception("User not found");

            if (!_cryptoService.Verify(user.PasswordHash, user.PasswordSalt, request.Password))
                throw new Exception("Wrong credential");

            if (!request.NewPassword.Equals(request.ConfirmNewPassword))
                throw new Exception("Password confimation doen't match new password");

            user.PasswordSalt = _cryptoService.GenerateSlat();
            user.PasswordHash = _cryptoService.GenerateHash(request.NewPassword, user.PasswordSalt);


            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }
    }
}
