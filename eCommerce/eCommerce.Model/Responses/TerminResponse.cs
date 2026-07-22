namespace eCommerce.Model.Responses
{
    public class TerminResponse
    {
        public int Id { get; set; }
        public int KlijentId { get; set; }
        public string? KlijentImePrezime { get; set; }
        public int FrizerId { get; set; }
        public string? FrizerImePrezime { get; set; }
        public int UslugaId { get; set; }
        public string? UslugaNaziv { get; set; }
        public DateTime DatumVrijeme { get; set; }
        public int TrajanjeMinuta { get; set; }
        public decimal Cijena { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Napomena { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
