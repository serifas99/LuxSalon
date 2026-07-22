using eCommerce.Common.Services.CryptoService;
using eCommerce.Common.Services.Messaging;
using eCommerce.Common.Services.Payments;
using eCommerce.Model.Requests;
using eCommerce.Model.Responses;
using eCommerce.Services;
using eCommerce.Services.Database;
using eCommerce.Services.ProductStateMachine;
using eCommerce.Services.Validators;
using eCommerce.WebAPI.Filters;
using eCommerce.WebAPI.Hubs;
using eCommerce.WebAPI.Services;
using eCommerce.WebAPI.Services.AccessManager;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Text;

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
TypeAdapterConfig<Product, ProductResponse>.NewConfig().IgnoreNullValues(true);
TypeAdapterConfig<Category, CategoryResponse>.NewConfig().IgnoreNullValues(true);
TypeAdapterConfig<User, UserResponse>.NewConfig().IgnoreNullValues(true);
TypeAdapterConfig<UserUpdateRequest, User>.NewConfig().IgnoreNullValues(true);
TypeAdapterConfig<ProductType, ProductTypeResponse>.NewConfig().IgnoreNullValues(true);
TypeAdapterConfig<UnitOfMeasure, UnitOfMeasureResponse>.NewConfig().IgnoreNullValues(true);
TypeAdapterConfig<Asset, AssetResponse>.NewConfig().IgnoreNullValues(true);
TypeAdapterConfig<ProductReview, ProductReviewResponse>.NewConfig()
    .Map(dest => dest.ReviewerDisplayName, src => $"{src.User.FirstName} {src.User.LastName}".Trim());
TypeAdapterConfig<Order, OrderResponse>.NewConfig()
    .Map(dest => dest.Status, src => (int)src.Status);
TypeAdapterConfig<OrderItem, OrderItemResponse>.NewConfig()
    .Map(dest => dest.ProductName, src => src.Product != null ? src.Product.Name : string.Empty);

// LuxSalon mappings
TypeAdapterConfig<UslugaKategorija, UslugaKategorijaResponse>.NewConfig().IgnoreNullValues(true);
TypeAdapterConfig<Usluga, UslugaResponse>.NewConfig()
    .Map(dest => dest.UslugaKategorijaNaziv, src => src.UslugaKategorija != null ? src.UslugaKategorija.Naziv : null);
TypeAdapterConfig<Frizer, FrizerResponse>.NewConfig()
    .Map(dest => dest.ImePrezime, src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}".Trim() : string.Empty)
    .Map(dest => dest.Email, src => src.User != null ? src.User.Email : null)
    .Map(dest => dest.UslugaIds, src => src.FrizerUsluge != null ? src.FrizerUsluge.Select(fu => fu.UslugaId).ToList() : new List<int>());
TypeAdapterConfig<Termin, TerminResponse>.NewConfig()
    .Map(dest => dest.Status, src => src.Status.ToString())
    .Map(dest => dest.KlijentImePrezime, src => src.Klijent != null ? $"{src.Klijent.FirstName} {src.Klijent.LastName}".Trim() : null)
    .Map(dest => dest.FrizerImePrezime, src => src.Frizer != null && src.Frizer.User != null ? $"{src.Frizer.User.FirstName} {src.Frizer.User.LastName}".Trim() : null)
    .Map(dest => dest.UslugaNaziv, src => src.Usluga != null ? src.Usluga.Naziv : null);
TypeAdapterConfig<Notifikacija, NotifikacijaResponse>.NewConfig()
    .Map(dest => dest.Tip, src => src.Tip.ToString());


// register application services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<BaseProductState>();
builder.Services.AddScoped<InitialProductState>();
builder.Services.AddScoped<DraftProductState>();
builder.Services.AddScoped<ActiveProductState>();

// category service
builder.Services.AddScoped<ICategoryService, CategoryService>();
// product type service
builder.Services.AddScoped<IProductTypeService, ProductTypeService>();
// unit of measure service
builder.Services.AddScoped<IUnitOfMeasureService, UnitOfMeasureService>();
// user service
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IAssetService, AssetService>();


builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

builder.Services.AddScoped<IAccessManager, AccessManager>();

builder.Services.AddScoped<ICryptoService, CryptoService>();

builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

builder.Services.AddScoped<eCommerce.Common.Services.Realtime.IRealtimeNotifier, SignalRRealtimeNotifier>();

builder.Services.AddHttpClient();
builder.Services.AddScoped<IPayPalClient, PayPalClient>();
builder.Services.AddScoped<IPlacanjeService, PlacanjeService>();

builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductReviewService, ProductReviewService>();

// LuxSalon services
builder.Services.AddScoped<IUslugaKategorijaService, UslugaKategorijaService>();
builder.Services.AddScoped<IUslugaService, UslugaService>();
builder.Services.AddScoped<IFrizerService, FrizerService>();
builder.Services.AddScoped<ITerminService, TerminService>();
builder.Services.AddScoped<INotifikacijaService, NotifikacijaService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();

builder.Services.AddScoped<IValidator<ProductTypeInsertRequest>, ProductTypeInsertValidator>();
builder.Services.AddScoped<IValidator<ProductTypeUpdateRequest>, ProductTypeUpdateValidator>();
builder.Services.AddScoped<IValidator<UnitOfMeasureInsertRequest>, UnitOfMeasureInsertValidator>();
builder.Services.AddScoped<IValidator<UnitOfMeasureUpdateRequest>, UnitOfMeasureUpdateValidator>();
builder.Services.AddScoped<IValidator<CategoriesInsertRequest>, CategoryInsertValidator>();
builder.Services.AddScoped<IValidator<CategoriesUpdateRequest>, CategoryUpdateValidator>();
builder.Services.AddScoped<IValidator<UserInsertRequest>, UserInsertValidator>();
builder.Services.AddScoped<IValidator<UserUpdateRequest>, UserUpdateValidator>();
builder.Services.AddScoped<IValidator<AssetInsertRequest>, AssetInsertValidator>();
builder.Services.AddScoped<IValidator<AssetUpdateRequest>, AssetUpdateValidator>();
builder.Services.AddScoped<IValidator<ProductReviewInsertRequest>, ProductReviewInsertValidator>();
builder.Services.AddScoped<IValidator<ProductReviewUpdateRequest>, ProductReviewUpdateValidator>();

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
