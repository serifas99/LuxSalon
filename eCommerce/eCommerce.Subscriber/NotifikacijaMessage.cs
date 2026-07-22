namespace eCommerce.Subscriber
{
    /// <summary>
    /// Oblik poruke koju eCommerce.WebAPI (RabbitMqPublisher) salje u "notifikacije" red.
    /// Namjerno je ovo posebna, jednostavna klasa (a ne referenca na eCommerce.Model) -
    /// worker je nezavisna aplikacija koja komunicira samo preko ugovora poruke (JSON).
    /// </summary>
    public class NotifikacijaMessage
    {
        public string Email { get; set; } = string.Empty;
        public string Naslov { get; set; } = string.Empty;
        public string Poruka { get; set; } = string.Empty;
        public DateTime Vrijeme { get; set; }
    }
}
