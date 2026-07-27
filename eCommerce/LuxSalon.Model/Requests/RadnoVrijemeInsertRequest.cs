namespace LuxSalon.Model.Requests
{
    public class RadnoVrijemeInsertRequest
    {
        public int FrizerId { get; set; }

        /// <summary>0 = Nedjelja, 1 = Ponedjeljak, ... 6 = Subota (isto kao System.DayOfWeek).</summary>
        public int DanUSedmici { get; set; }

        /// <summary>Format "HH:mm", npr. "08:00".</summary>
        public string PocetakRada { get; set; } = "08:00";

        /// <summary>Format "HH:mm", npr. "17:00".</summary>
        public string KrajRada { get; set; } = "17:00";

        public bool NeRadi { get; set; } = false;
    }
}
