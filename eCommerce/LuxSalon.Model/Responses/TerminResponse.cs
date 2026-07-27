namespace LuxSalon.Model.Responses
{
    public class TerminResponse
    {
        public int Id { get; set; }
        public int KlijentId { get; set; }
        public string? KlijentImePrezime { get; set; }
        public int FrizerId { get; set; }
        public string? FrizerImePrezime { get; set; }
        public int UslugaId { get; set; }
        public string? UslugaNaziv { get; set; }
        public DateTime DatumVrijeme { get; set; }
        public int TrajanjeMinuta { get; set; }
        public decimal Cijena { get; set; }
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Status vezanog Placanja (npr. "Zavrseno") ako postoji, inace null - koriste ga klijenti
        /// (mobilna app) da odluce da li jos treba prikazati dugme "Plati" (termin moze biti
        /// "Potvrdjen" i bez placanja, ako ga frizer/admin rucno potvrdi).
        /// </summary>
        public string? PlacanjeStatus { get; set; }

        /// <summary>
        /// Id vezanog Placanja (ako postoji) - koristi ga desktop app (Admin/Frizer) da pozove
        /// POST Placanje/{id}/Vrati kad treba refundovati zavrseno placanje za ovaj termin.
        /// </summary>
        public int? PlacanjeId { get; set; }

        public string? Napomena { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
