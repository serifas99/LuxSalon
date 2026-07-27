namespace LuxSalon.Model.Requests
{
    // Koristi se samo za pomjeranje termina (datum/vrijeme) i napomenu.
    // Promjena statusa ide preko posebnih akcija (Potvrdi/Otkazi/OznaciOdradjen...).
    public class TerminUpdateRequest
    {
        public DateTime DatumVrijeme { get; set; }
        public string? Napomena { get; set; }
    }
}
