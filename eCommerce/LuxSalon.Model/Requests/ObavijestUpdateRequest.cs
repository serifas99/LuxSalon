namespace LuxSalon.Model.Requests
{
    public class ObavijestUpdateRequest
    {
        public string Naslov { get; set; } = string.Empty;
        public string Tekst { get; set; } = string.Empty;
        public string? SlikaBase64 { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
