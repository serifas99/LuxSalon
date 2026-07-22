namespace eCommerce.Model.Responses
{
    public class PlacanjeResponse
    {
        public int Id { get; set; }
        public int TerminId { get; set; }
        public decimal Iznos { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? PaypalOrderId { get; set; }
        public string? PaypalTransactionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DatumPlacanja { get; set; }
        public DateTime? DatumPovrata { get; set; }
    }
}
