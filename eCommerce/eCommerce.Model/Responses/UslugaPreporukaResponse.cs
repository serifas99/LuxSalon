namespace eCommerce.Model.Responses
{
    public class UslugaPreporukaResponse
    {
        public UslugaResponse Usluga { get; set; } = null!;

        /// <summary>
        /// Konacni hibridni skor preporuke (0-1), veci = relevantnije.
        /// </summary>
        public double Skor { get; set; }

        public double ContentBasedSkor { get; set; }

        public double PopularityBasedSkor { get; set; }

        /// <summary>
        /// Ljudski citljivo objasnjenje zasto je usluga preporucena (prikazuje se korisniku).
        /// </summary>
        public string Objasnjenje { get; set; } = string.Empty;
    }
}
