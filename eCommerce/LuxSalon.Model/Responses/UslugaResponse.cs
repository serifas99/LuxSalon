namespace LuxSalon.Model.Responses
{
    public class UslugaResponse
    {
        public int Id { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public string Opis { get; set; } = string.Empty;
        public decimal Cijena { get; set; }
        public int TrajanjeMinuta { get; set; }
        public int? UslugaKategorijaId { get; set; }
        public string? UslugaKategorijaNaziv { get; set; }
        public string Tagovi { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
