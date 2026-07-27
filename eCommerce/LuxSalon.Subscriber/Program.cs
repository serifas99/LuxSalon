using LuxSalon.Subscriber;

// Ucitava .env fajl (ako postoji) prije citanja konfiguracije - RabbitMQ kredencijali
// se drze van appsettings.json (vidi LuxSalon.WebAPI/Program.cs za isti pristup).
DotNetEnv.Env.Load();

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
