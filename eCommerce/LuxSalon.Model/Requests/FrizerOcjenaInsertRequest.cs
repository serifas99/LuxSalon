namespace LuxSalon.Model.Requests
{
    /// <summary>
    /// KlijentId i FrizerId se NE primaju od klijenta - server ih odredjuje iz JWT tokena
    /// (KlijentId) i iz samog termina (FrizerId), da korisnik ne bi mogao ocijeniti u tudje ime.
    /// </summary>
    public class FrizerOcjenaInsertRequest
    {
        public int TerminId { get; set; }
        public int Ocjena { get; set; }
        public string? Komentar { get; set; }
    }
}
