namespace eCommerce.Model.Requests
{
    public class FrizerInsertRequest
    {
        public int UserId { get; set; }
        public string? Biografija { get; set; }
        public string? Specijalizacija { get; set; }
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Id-jevi usluga koje frizer moze izvoditi.
        /// </summary>
        public List<int> UslugaIds { get; set; } = new List<int>();
    }
}
