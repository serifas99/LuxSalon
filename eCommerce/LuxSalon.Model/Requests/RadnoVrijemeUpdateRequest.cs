namespace LuxSalon.Model.Requests
{
    public class RadnoVrijemeUpdateRequest
    {
        public string PocetakRada { get; set; } = "08:00";
        public string KrajRada { get; set; } = "17:00";
        public bool NeRadi { get; set; } = false;
    }
}
