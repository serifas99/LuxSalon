namespace eCommerce.Model.SearchObjects
{
    public class TerminSearchObject : BaseSearchObject
    {
        public int? KlijentId { get; set; }
        public int? FrizerId { get; set; }
        public int? UslugaId { get; set; }

        /// <summary>Filtrira termine po statusu (underlying int vrijednost TerminStatus enuma).</summary>
        public int? Status { get; set; }
        public DateTime? OdDatuma { get; set; }
        public DateTime? DoDatuma { get; set; }
    }
}
