namespace LuxSalon.Common.Services.Messaging
{
    public interface IRabbitMqPublisher
    {
        /// <summary>
        /// Salje poruku u "notifikacije" red. Ne baca izuzetak ako RabbitMQ nije dostupan -
        /// samo loguje upozorenje, da ne bi srusio glavnu operaciju (npr. zakazivanje termina)
        /// ako Docker/RabbitMQ nije pokrenut.
        /// </summary>
        void PublishNotifikacija(string email, string naslov, string poruka);
    }
}
