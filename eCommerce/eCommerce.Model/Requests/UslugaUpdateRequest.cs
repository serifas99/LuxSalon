namespace eCommerce.Model.Requests
{
    public class UslugaUpdateRequest
    {
        public string Naziv { get; set; } = string.Empty;
        public string Opis { get; set; } = string.Empty;
        public decimal Cijena { get; set; }
        public int TrajanjeMinuta { get; set; }
        public int? UslugaKategorijaId { get; set; }
        public string Tagovi { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
