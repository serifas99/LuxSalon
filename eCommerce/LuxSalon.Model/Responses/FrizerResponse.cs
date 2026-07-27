namespace LuxSalon.Model.Responses
{
    public class FrizerResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ImePrezime { get; set; } = string.Empty;
        public string? Email { get; set; }

        /// <summary>
        /// Slika profila (base64) - dolazi sa korisnickog naloga (User.ProfileImageBase64),
        /// isto polje koje klijent moze urediti u svom profilu na mobile-u.
        /// </summary>
        public string? ProfileImageBase64 { get; set; }

        public string? Biografija { get; set; }
        public string? Specijalizacija { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<int> UslugaIds { get; set; } = new List<int>();
    }
}
