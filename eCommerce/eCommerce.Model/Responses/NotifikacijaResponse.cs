namespace eCommerce.Model.Responses
{
    public class NotifikacijaResponse
    {
        public int Id { get; set; }
        public int KorisnikId { get; set; }
        public string Naslov { get; set; } = string.Empty;
        public string Poruka { get; set; } = string.Empty;
        public string Tip { get; set; } = string.Empty;
        public bool Procitano { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? TerminId { get; set; }
    }
}
