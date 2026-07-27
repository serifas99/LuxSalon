using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace LuxSalon.Common.Services.Messaging
{
    /// <summary>
    /// Objavljuje poruke o notifikacijama (npr. "termin potvrdjen") u RabbitMQ red "notifikacije".
    /// LuxSalon.Subscriber (posebna worker aplikacija) cita taj red i salje email preko MailHog-a.
    /// Konekcija se pravi lijeno (tek kad se prvi put nesto objavljuje) i sve greske se hvataju
    /// da RabbitMQ ne bude tacka pucanja za ostatak aplikacije.
    /// </summary>
    public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
    {
        public const string QueueName = "notifikacije";

        private readonly IConfiguration _configuration;
        private readonly ILogger<RabbitMqPublisher> _logger;

        private IConnection? _connection;
        private IModel? _channel;
        private readonly object _lock = new();

        public RabbitMqPublisher(IConfiguration configuration, ILogger<RabbitMqPublisher> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public void PublishNotifikacija(string email, string naslov, string poruka)
        {
            try
            {
                EnsureChannel();

                var payload = new
                {
                    Email = email,
                    Naslov = naslov,
                    Poruka = poruka,
                    Vrijeme = DateTime.UtcNow
                };

                var json = JsonSerializer.Serialize(payload);
                var body = Encoding.UTF8.GetBytes(json);

                var properties = _channel!.CreateBasicProperties();
                properties.Persistent = true;
                properties.ContentType = "application/json";

                _channel.BasicPublish(exchange: "", routingKey: QueueName, basicProperties: properties, body: body);
            }
            catch (Exception ex)
            {
                // Namjerno ne bacamo dalje - notifikacija je "best effort", ne smije srusiti npr. zakazivanje termina.
                _logger.LogWarning(ex, "Nije moguce objaviti notifikaciju u RabbitMQ (da li je Docker/RabbitMQ pokrenut?).");
            }
        }

        private void EnsureChannel()
        {
            if (_channel is { IsOpen: true })
                return;

            lock (_lock)
            {
                if (_channel is { IsOpen: true })
                    return;

                var factory = new ConnectionFactory
                {
                    HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
                    Port = int.TryParse(_configuration["RabbitMQ:Port"], out var port) ? port : 5672,
                    UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
                    Password = _configuration["RabbitMQ:Password"] ?? "guest"
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            }
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
