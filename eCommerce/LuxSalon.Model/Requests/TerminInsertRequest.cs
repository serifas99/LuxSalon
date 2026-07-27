namespace LuxSalon.Model.Requests
{
    public class TerminInsertRequest
    {
        public int KlijentId { get; set; }
        public int FrizerId { get; set; }
        public int UslugaId { get; set; }
        public DateTime DatumVrijeme { get; set; }
        public string? Napomena { get; set; }
    }
}
