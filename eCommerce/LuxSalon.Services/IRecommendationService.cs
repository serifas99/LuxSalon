using LuxSalon.Model.Responses;

namespace LuxSalon.Services
{
    public interface IRecommendationService
    {
        Task<List<UslugaPreporukaResponse>> PreporuciAsync(int klijentId, int brojPreporuka = 5);
    }
}
