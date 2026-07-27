namespace LuxSalon.Model.Responses
{
    public class ObavijestResponse
    {
        public int Id { get; set; }
        public string Naslov { get; set; } = string.Empty;
        public string Tekst { get; set; } = string.Empty;
        public string? SlikaBase64 { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
