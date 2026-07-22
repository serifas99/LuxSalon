namespace eCommerce.Model.SearchObjects
{
    public class UslugaSearchObject : BaseSearchObject
    {
        public string? Naziv { get; set; }
        public int? UslugaKategorijaId { get; set; }
        public bool? IsActive { get; set; }

        /// <summary>
        /// Tag za content-based pretragu/preporuku (npr. "kosa").
        /// </summary>
        public string? Tag { get; set; }
    }
}
