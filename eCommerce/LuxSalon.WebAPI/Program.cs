using LuxSalon.Common.Services.CryptoService;
using LuxSalon.Common.Services.Messaging;
using LuxSalon.Common.Services.Payments;
using LuxSalon.Model.Requests;
using LuxSalon.Model.Responses;
using LuxSalon.Services;
using LuxSalon.Services.Database;
using LuxSalon.Services.Validators;
using LuxSalon.WebAPI.Filters;
using LuxSalon.WebAPI.Hubs;
using LuxSalon.WebAPI.Services;
using LuxSalon.WebAPI.Services.AccessManager;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Text;

// Ucitava .env fajl (ako postoji) u process environment varijable PRIJE nego sto
// WebApplicationBuilder procita konfiguraciju - tajne (connection string, JWT, RabbitMQ,
// PayPal) se drze van appsettings.json po uputstvu za seminarski rad. Env varijable
// koriste "Section__Key" notaciju (npr. JwtToken__SecretKey) da bi automatski nadjacale
// odgovarajuce appsettings.json vrijednosti - ovo radi standardni ASP.NET Core
// konfiguracioni provider za environment varijable, koji se dodaje automatski.
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthenticatedUserAccessor, HttpAuthenticatedUserAccessor>();

builder.Services.AddControllers(
   options => options.Filters.Add<ExceptionFilter>()
);

// Add Entity Framework Core DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ECommerceDbContext>(options =>
    options.UseSqlServer(connectionString)
);

// register Mapster for object mapping
builder.Services.AddMapster();

// configure a few mappings explicitly if needed (optional)
// Mapster will automatically map same-named properties, but configuration
// ensures any custom rules or future needs can be added here.
// Role se ne mapira automatski (User.UserRoles je lista, UserResponse.Role je string) - bez
// ovoga generickie GetAll (npr. dropdown za biranje korisnika kod dodavanja frizera) vraca
// Role = null za svakog korisnika, sto kvari bilo kakvo filtriranje po roli na klijentu.
TypeAdapterConfig<User, UserResponse>.NewConfig()
    .Map(dest => dest.Role, src => src.UserRoles.Select(ur => ur.Role.Name).FirstOrDefault())
    .IgnoreNullValues(true);
TypeAdapterConfig<UserUpdateRequest, User>.NewConfig().IgnoreNullValues(true);

// LuxSalon mappings
TypeAdapterConfig<UslugaKategorija, UslugaKategorijaResponse>.NewConfig().IgnoreNullValues(true);
TypeAdapterConfig<Usluga, UslugaResponse>.NewConfig()
    .Map(dest => dest.UslugaKategorijaNaziv, src => src.UslugaKategorija != null ? src.UslugaKategorija.Naziv : null);
TypeAdapterConfig<Frizer, FrizerResponse>.NewConfig()
    .Map(dest => dest.ImePrezime, src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}".Trim() : string.Empty)
    .Map(dest => dest.Email, src => src.User != null ? src.User.Email : null)
    .Map(dest => dest.ProfileImageBase64, src => src.User != null ? src.User.ProfileImageBase64 : null)
    .Map(dest => dest.UslugaIds, src => src.FrizerUsluge != null ? src.FrizerUsluge.Select(fu => fu.UslugaId).ToList() : new List<int>());
TypeAdapterConfig<Termin, TerminResponse>.NewConfig()
    .Map(dest => dest.Status, src => src.Status.ToString())
    .Map(dest => dest.KlijentImePrezime, src => src.Klijent != null ? $"{src.Klijent.FirstName} {src.Klijent.LastName}".Trim() : null)
    .Map(dest => dest.FrizerImePrezime, src => src.Frizer != null && src.Frizer.User != null ? $"{src.Frizer.User.FirstName} {src.Frizer.User.LastName}".Trim() : null)
    .Map(dest => dest.UslugaNaziv, src => src.Usluga != null ? src.Usluga.Naziv : null)
    .Map(dest => dest.PlacanjeStatus, src => src.Placanje != null ? src.Placanje.Status.ToString() : null)
    .Map(dest => dest.PlacanjeId, src => src.Placanje != null ? (int?)src.Placanje.Id : null);
TypeAdapterConfig<Notifikacija, NotifikacijaResponse>.NewConfig()
    .Map(dest => dest.Tip, src => src.Tip.ToString());
TypeAdapterConfig<FrizerOcjena, FrizerOcjenaResponse>.NewConfig()
    .Map(dest => dest.KlijentImePrezime, src => src.Klijent != null ? $"{src.Klijent.FirstName} {src.Klijent.LastName}".Trim() : null)
    .Map(dest => dest.FrizerImePrezime, src => src.Frizer != null && src.Frizer.User != null ? $"{src.Frizer.User.FirstName} {src.Frizer.User.LastName}".Trim() : null);
TypeAdapterConfig<RadnoVrijeme, RadnoVrijemeResponse>.NewConfig()
    .Map(dest => dest.FrizerImePrezime, src => src.Frizer != null && src.Frizer.User != null ? $"{src.Frizer.User.FirstName} {src.Frizer.User.LastName}".Trim() : null)
    .Map(dest => dest.DanUSedmici, src => (int)src.DanUSedmici)
    .Map(dest => dest.DanUSedmiceNaziv, src => src.DanUSedmici == DayOfWeek.Monday ? "Ponedjeljak" :
        src.DanUSedmici == DayOfWeek.Tuesday ? "Utorak" :
        src.DanUSedmici == DayOfWeek.Wednesday ? "Srijeda" :
        src.DanUSedmici == DayOfWeek.Thursday ? "Cetvrtak" :
        src.DanUSedmici == DayOfWeek.Friday ? "Petak" :
        src.DanUSedmici == DayOfWeek.Saturday ? "Subota" : "Nedjelja")
    .Map(dest => dest.PocetakRada, src => src.PocetakRada.ToString(@"hh\:mm"))
    .Map(dest => dest.KrajRada, src => src.KrajRada.ToString(@"hh\:mm"));
TypeAdapterConfig<IstorijaStatusaTermina, IstorijaStatusaTerminaResponse>.NewConfig()
    .Map(dest => dest.PrethodniStatus, src => src.PrethodniStatus.ToString())
    .Map(dest => dest.NoviStatus, src => src.NoviStatus.ToString())
    .Map(dest => dest.PromijenioKorisnikImePrezime, src => src.PromijenioKorisnik != null ? $"{src.PromijenioKorisnik.FirstName} {src.PromijenioKorisnik.LastName}".Trim() : null);


// register application services
// user service
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

builder.Services.AddScoped<IAccessManager, AccessManager>();

builder.Services.AddScoped<ICryptoService, CryptoService>();

builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

builder.Services.AddScoped<LuxSalon.Common.Services.Realtime.IRealtimeNotifier, SignalRRealtimeNotifier>();

builder.Services.AddHttpClient();
builder.Services.AddScoped<IPayPalClient, PayPalClient>();
builder.Services.AddScoped<IPlacanjeService, PlacanjeService>();

// LuxSalon services
builder.Services.AddScoped<IUslugaKategorijaService, UslugaKategorijaService>();
builder.Services.AddScoped<IUslugaService, UslugaService>();
builder.Services.AddScoped<IFrizerService, FrizerService>();
builder.Services.AddScoped<ITerminService, TerminService>();
builder.Services.AddScoped<INotifikacijaService, NotifikacijaService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<IFrizerOcjenaService, FrizerOcjenaService>();
builder.Services.AddScoped<IRadnoVrijemeService, RadnoVrijemeService>();
builder.Services.AddScoped<IIstorijaStatusaTerminaService, IstorijaStatusaTerminaService>();
builder.Services.AddScoped<IObavijestService, ObavijestService>();

builder.Services.AddScoped<IValidator<UserInsertRequest>, UserInsertValidator>();
builder.Services.AddScoped<IValidator<UserUpdateRequest>, UserUpdateValidator>();

// LuxSalon validators
builder.Services.AddScoped<IValidator<UslugaKategorijaInsertRequest>, UslugaKategorijaInsertValidator>();
builder.Services.AddScoped<IValidator<UslugaKategorijaUpdateRequest>, UslugaKategorijaUpdateValidator>();
builder.Services.AddScoped<IValidator<UslugaInsertRequest>, UslugaInsertValidator>();
builder.Services.AddScoped<IValidator<UslugaUpdateRequest>, UslugaUpdateValidator>();
builder.Services.AddScoped<IValidator<FrizerInsertRequest>, FrizerInsertValidator>();
builder.Services.AddScoped<IValidator<FrizerUpdateRequest>, FrizerUpdateValidator>();
builder.Services.AddScoped<IValidator<TerminInsertRequest>, TerminInsertValidator>();
builder.Services.AddScoped<IValidator<TerminUpdateRequest>, TerminUpdateValidator>();
builder.Services.AddScoped<IValidator<NotifikacijaInsertRequest>, NotifikacijaInsertValidator>();
builder.Services.AddScoped<IValidator<NotifikacijaUpdateRequest>, NotifikacijaUpdateValidator>();
builder.Services.AddScoped<IValidator<FrizerOcjenaInsertRequest>, FrizerOcjenaInsertValidator>();
builder.Services.AddScoped<IValidator<FrizerOcjenaUpdateRequest>, FrizerOcjenaUpdateValidator>();
builder.Services.AddScoped<IValidator<RadnoVrijemeInsertRequest>, RadnoVrijemeInsertValidator>();
builder.Services.AddScoped<IValidator<RadnoVrijemeUpdateRequest>, RadnoVrijemeUpdateValidator>();
builder.Services.AddScoped<IValidator<ObavijestInsertRequest>, ObavijestInsertValidator>();
builder.Services.AddScoped<IValidator<ObavijestUpdateRequest>, ObavijestUpdateValidator>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAuthentication(options => // dodavanje authentfikacije i autorizacije u projekat
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["JwtToken:Issuer"],
        ValidAudience = builder.Configuration["JwtToken:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtToken:SecretKey"] ?? string.Empty)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero
    };

    // SignalR (WebSocket) konekcije ne mogu slati Authorization header na handshake,
    // pa Flutter klijenti salju JWT kao ?access_token=... u query stringu - ovdje ga
    // "prebacujemo" da standardna JWT validacija iznad i dalje radi kao i za obicne API pozive.
    o.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorization();

builder.Services.AddSignalR();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Version = "v1",
            Title = "LuxSalon API",
            Description = "API za upravljanje uslugama, frizerima i terminima u LuxSalon aplikaciji"
        });

        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));

        var jwtSecurityScheme = new OpenApiSecurityScheme
        {
            BearerFormat = "JWT",
            Name = "JWT Authentication",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            Reference = new OpenApiReference
            {
                Id = JwtBearerDefaults.AuthenticationScheme,
                Type = ReferenceType.SecurityScheme
            }
        };

        options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { jwtSecurityScheme, Array.Empty<string>() }
                });
    });

// Dozvoljava Flutter web (browser) build-u da poziva API - desktop/mobile (native) build ne prolazi kroz CORS,
// ali web build izvrsava fetch pozive iz browsera pa treba eksplicitnu dozvolu. OK za lokalni razvoj/seminarski.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Automatski primijeni EF Core migracije (kreira bazu + tabele + seed podatke ako ne postoje)
// pri startu aplikacije - tako profesor/asistent ne mora rucno pokretati
// "dotnet ef database update" da bi projekat proradio nakon kloniranja repozitorija.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();


    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<NotifikacijaHub>("/hubs/notifikacije");

app.Run();
