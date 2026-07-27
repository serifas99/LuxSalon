namespace LuxSalon.Model.Responses
{
    /// <summary>
    /// Pregled klijenta za desktop "Klijenti" ekran (tacno po prijavi teme): ime i prezime,
    /// email, broj zakazanih termina i podaci o posljednjem terminu.
    /// </summary>
    public class KlijentPregledResponse
    {
        public int Id { get; set; }
        public string ImePrezime { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int BrojZakazanihTermina { get; set; }
        public DateTime? DatumPosljednjegTermina { get; set; }
    }
}
