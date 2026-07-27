using LuxSalon.Model.Responses;

namespace LuxSalon.Services
{
    public interface IPlacanjeService
    {
        Task<PlacanjeKreirajResponse> KreirajAsync(int terminId, string backendBaseUrl);
        Task<PlacanjeResponse> PotvrdiAsync(string paypalOrderId);
        Task<PlacanjeResponse> VratiNovacAsync(int placanjeId);
        Task<PlacanjeResponse> GetByIdAsync(int id);
    }
}
