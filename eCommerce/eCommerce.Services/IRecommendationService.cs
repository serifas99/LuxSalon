using eCommerce.Model.Responses;

namespace eCommerce.Services
{
    public interface IRecommendationService
    {
        Task<List<UslugaPreporukaResponse>> PreporuciAsync(int klijentId, int brojPreporuka = 5);
    }
}
