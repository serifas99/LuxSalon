namespace LuxSalon.Model.Responses
{
    public class FrizerOcjenaResponse
    {
        public int Id { get; set; }
        public int TerminId { get; set; }
        public int KlijentId { get; set; }
        public string? KlijentImePrezime { get; set; }
        public int FrizerId { get; set; }
        public string? FrizerImePrezime { get; set; }
        public int Ocjena { get; set; }
        public string? Komentar { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
