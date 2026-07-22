namespace eCommerce.Model.Requests
{
    public class UslugaKategorijaInsertRequest
    {
        public string Naziv { get; set; } = string.Empty;
        public string Opis { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
