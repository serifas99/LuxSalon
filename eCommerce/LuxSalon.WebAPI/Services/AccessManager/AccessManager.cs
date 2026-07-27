using LuxSalon.Common.Services.CryptoService;
using LuxSalon.Common.Services.Messaging;
using LuxSalon.Model.Access;
using LuxSalon.Model.Exceptions;
using LuxSalon.Model.Responses;
using LuxSalon.Services;
using LuxSalon.Services.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LuxSalon.WebAPI.Services.AccessManager
{
    public class AccessManager : IAccessManager
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly ICryptoService _cryptoService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ECommerceDbContext _dbContext;
        private readonly IRabbitMqPublisher _rabbitMqPublisher;

        private const int KodIstekMinuta = 15;

        public AccessManager(IUserService userService, IConfiguration configuration, ICryptoService cryptoService, IRefreshTokenService refreshTokenService, ECommerceDbContext dbContext, IRabbitMqPublisher rabbitMqPublisher)
        {
            _userService = userService;
            _configuration = configuration;
            _cryptoService = cryptoService;
            _refreshTokenService = refreshTokenService;
            _dbContext = dbContext;
            _rabbitMqPublisher = rabbitMqPublisher;
        }

        public async Task<UserLoginResponse> LoginAsync(UserLoginRequest request)
        {
            var user = await _userService.GetByUsernameAsync(request.Username);


            if (user == null)
            {
                throw new ClinetException("Pogrešno korisničko ime ili lozinka.");
            }

            var validPassword = _cryptoService.Verify(user.PasswordHash, user.PasswordSalt, request.Password);
            if (!validPassword)
            {
                throw new ClinetException("Pogrešno korisničko ime ili lozinka.");
            }

            var accessToken = GenerateToken(user);
            var refreshTokenValue = GenerateRefreshToken();

            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshTokenValue,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            await _refreshTokenService.InsertAsync(refreshToken);

            return new UserLoginResponse
            {
                Accesstoken = accessToken,
                Refreshtoken = refreshTokenValue
            };
        }

        public async Task<UserLoginResponse> LoginWithRefreshTokenAsync(RefreshAccessTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                throw new ClinetException("Refresh token is required");
            }

            var refreshToken = await _refreshTokenService.GetStoredTokenAsync(request.RefreshToken);

            if (refreshToken == null)
            {
                throw new ClinetException("Invalid refresh token");
            }

            if (refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                throw new ClinetException("Refresh token has expired");
            }

            var user = await _userService.GetWithRoleByIdAsync(refreshToken.UserId);

            if (user == null)
            {
                throw new ClinetException("User not found");
            }

            if (!user.IsActive)
            {
                throw new ClinetException("User is not active");
            }

            await _refreshTokenService.DeleteAllUserRefreshTokensAsync(user.Id);

            var accessToken = GenerateToken(user);
            var refreshTokenValue = GenerateRefreshToken();

            var token = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshTokenValue,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            await _refreshTokenService.InsertAsync(token);

            return new UserLoginResponse
            {
                Accesstoken = accessToken,
                Refreshtoken = refreshTokenValue
            };

        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var email = (request.Email ?? string.Empty).Trim();
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

            // Namjerno ne otkrivamo da li email postoji u sistemu (isti odgovor u oba slucaja) -
            // email se stvarno salje samo ako korisnik zaista postoji i aktivan je.
            if (user == null || !user.IsActive)
                return;

            // Prethodni neiskorisceni kodovi za ovog korisnika vise ne vrijede - samo posljednji
            // zatrazeni kod smije biti validan.
            var stariKodovi = await _dbContext.PasswordResetCodes
                .Where(k => k.UserId == user.Id && !k.IsUsed)
                .ToListAsync();
            foreach (var stari in stariKodovi)
                stari.IsUsed = true;

            // 6-cifreni numericki kod generisan kriptografski sigurnim generatorom (ne System.Random).
            var kod = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            var salt = _cryptoService.GenerateSlat();
            var hash = _cryptoService.GenerateHash(kod, salt);

            _dbContext.PasswordResetCodes.Add(new PasswordResetCode
            {
                UserId = user.Id,
                CodeHash = hash,
                CodeSalt = salt,
                ExpiresAt = DateTime.UtcNow.AddMinutes(KodIstekMinuta),
                IsUsed = false
            });

            await _dbContext.SaveChangesAsync();

            _rabbitMqPublisher.PublishNotifikacija(
                user.Email,
                "Reset lozinke - LuxSalon",
                $"Vaš kod za reset lozinke je: {kod}. Kod ističe za {KodIstekMinuta} minuta. Ako niste vi zatražili reset lozinke, zanemarite ovaj email.");
        }

        public async Task ResetPasswordAsync(ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword != request.ConfirmNewPassword)
                throw new ClinetException("Nova lozinka i potvrda lozinke se ne podudaraju.");

            if (request.NewPassword.Length < 6)
                throw new ClinetException("Nova lozinka mora imati najmanje 6 karaktera.");

            var email = (request.Email ?? string.Empty).Trim();
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                throw new ClinetException("Neispravan ili istekao kod.");

            var kandidati = await _dbContext.PasswordResetCodes
                .Where(k => k.UserId == user.Id && !k.IsUsed && k.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(k => k.CreatedAt)
                .ToListAsync();

            var validan = kandidati.FirstOrDefault(k => _cryptoService.Verify(k.CodeHash, k.CodeSalt, request.Code ?? string.Empty));
            if (validan == null)
                throw new ClinetException("Neispravan ili istekao kod.");

            validan.IsUsed = true;

            var novaSalt = _cryptoService.GenerateSlat();
            var noviHash = _cryptoService.GenerateHash(request.NewPassword, novaSalt);
            user.PasswordSalt = novaSalt;
            user.PasswordHash = noviHash;

            await _dbContext.SaveChangesAsync();
        }

        private string GenerateToken(UserResponse user)
        {
            string secretKeyString = _configuration["JwtToken:SecretKey"] ?? string.Empty;
            var issuer = _configuration["JwtToken:Issuer"];
            var audience = _configuration["JwtToken:Audience"];
            var durationInMinutes = int.Parse(_configuration["JwtToken:DurationInMinutes"] ?? "1");

            var secretKey = Encoding.ASCII.GetBytes(secretKeyString);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimNames.Id, user.Id.ToString()),
                    new Claim(ClaimNames.FirstName, user.FirstName ?? string.Empty),
                    new Claim(ClaimNames.LastName, user.LastName ?? string.Empty),
                    new Claim(ClaimNames.Email, user.Email ?? string.Empty),
                    new Claim(ClaimNames.Role, user.Role ?? "user"),
                    new Claim(ClaimNames.IsActive, user.IsActive.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(durationInMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            var randombytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randombytes);
        }

       
    }
}
