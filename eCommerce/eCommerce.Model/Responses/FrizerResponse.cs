namespace eCommerce.Model.Responses
{
    public class FrizerResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ImePrezime { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Biografija { get; set; }
        public string? Specijalizacija { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<int> UslugaIds { get; set; } = new List<int>();
    }
}
