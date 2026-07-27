namespace LuxSalon.Model.Requests
{
    // Koristi se samo za oznacavanje kao procitano/nepromitano preko generickog Update-a.
    // Za obicno "oznaci procitano" postoji i posebna akcija na kontroleru.
    public class NotifikacijaUpdateRequest
    {
        public bool Procitano { get; set; }
    }
}
