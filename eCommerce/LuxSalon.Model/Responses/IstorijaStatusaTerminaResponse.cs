namespace LuxSalon.Model.Responses
{
    public class IstorijaStatusaTerminaResponse
    {
        public int Id { get; set; }
        public int TerminId { get; set; }
        public string PrethodniStatus { get; set; } = string.Empty;
        public string NoviStatus { get; set; } = string.Empty;
        public int PromijenioKorisnikId { get; set; }
        public string? PromijenioKorisnikImePrezime { get; set; }
        public string? Opis { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
