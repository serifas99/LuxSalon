using System.Net.Mail;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace eCommerce.Subscriber
{
    /// <summary>
    /// Background servis koji slusa RabbitMQ red "notifikacije" i za svaku poruku salje email
    /// preko lokalnog SMTP-a (MailHog u razvoju - emailovi se vide na http://localhost:8025).
    /// </summary>
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private IConnection? _connection;
        private IModel? _channel;

        private const string QueueName = "notifikacije";

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            PovezivanjeSaRabbitMq();
            return base.StartAsync(cancellationToken);
        }

        private void PovezivanjeSaRabbitMq()
        {
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

            _logger.LogInformation("Povezan na RabbitMQ, slusam red '{QueueName}'.", QueueName);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_channel == null)
            {
                _logger.LogError("RabbitMQ kanal nije inicijalizovan.");
                return Task.CompletedTask;
            }

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                try
                {
                    var poruka = JsonSerializer.Deserialize<NotifikacijaMessage>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (poruka != null)
                    {
                        PosaljiEmail(poruka);
                    }

                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Greska pri obradi notifikacije: {Json}", json);
                    // Ne vracamo poruku u red (nema smisla ponavljati istu neuspjelu email poruku unedogled)
                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                }
            };

            _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

            return Task.CompletedTask;
        }

        private void PosaljiEmail(NotifikacijaMessage poruka)
        {
            var smtpHost = _configuration["Smtp:Host"] ?? "localhost";
            var smtpPort = int.TryParse(_configuration["Smtp:Port"], out var p) ? p : 1025;
            var fromAddress = _configuration["Smtp:FromAddress"] ?? "no-reply@luxsalon.com";
            var fromName = _configuration["Smtp:FromName"] ?? "LuxSalon";

            using var client = new SmtpClient(smtpHost, smtpPort);

            using var mail = new MailMessage
            {
                From = new MailAddress(fromAddress, fromName),
                Subject = poruka.Naslov,
                Body = poruka.Poruka,
                IsBodyHtml = false
            };
            mail.To.Add(poruka.Email);

            client.Send(mail);

            _logger.LogInformation("Email poslan na {Email}: {Naslov}", poruka.Email, poruka.Naslov);
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
