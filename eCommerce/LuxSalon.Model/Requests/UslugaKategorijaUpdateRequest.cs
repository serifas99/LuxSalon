namespace LuxSalon.Model.Requests
{
    public class UslugaKategorijaUpdateRequest
    {
        public string Naziv { get; set; } = string.Empty;
        public string Opis { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
