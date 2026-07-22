namespace eCommerce.Model.Requests
{
    public class NotifikacijaInsertRequest
    {
        public int KorisnikId { get; set; }
        public string Naslov { get; set; } = string.Empty;
        public string Poruka { get; set; } = string.Empty;
        public int Tip { get; set; }
        public int? TerminId { get; set; }
    }
}
