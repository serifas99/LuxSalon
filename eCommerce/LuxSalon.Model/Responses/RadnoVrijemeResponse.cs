namespace LuxSalon.Model.Responses
{
    public class RadnoVrijemeResponse
    {
        public int Id { get; set; }
        public int FrizerId { get; set; }
        public string? FrizerImePrezime { get; set; }
        public int DanUSedmici { get; set; }
        public string DanUSedmiceNaziv { get; set; } = string.Empty;
        public string PocetakRada { get; set; } = string.Empty;
        public string KrajRada { get; set; } = string.Empty;
        public bool NeRadi { get; set; }
    }
}
