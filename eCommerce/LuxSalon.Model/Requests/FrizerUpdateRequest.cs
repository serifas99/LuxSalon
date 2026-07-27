namespace LuxSalon.Model.Requests
{
    public class FrizerUpdateRequest
    {
        public string? Biografija { get; set; }
        public string? Specijalizacija { get; set; }
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Id-jevi usluga koje frizer moze izvoditi (zamjenjuje postojecu listu).
        /// </summary>
        public List<int> UslugaIds { get; set; } = new List<int>();
    }
}
