using eCommerce.Model.Responses;

namespace eCommerce.Services
{
    public interface IPlacanjeService
    {
        Task<PlacanjeKreirajResponse> KreirajAsync(int terminId);
        Task<PlacanjeResponse> PotvrdiAsync(string paypalOrderId);
        Task<PlacanjeResponse> VratiNovacAsync(int placanjeId);
        Task<PlacanjeResponse> GetByIdAsync(int id);
    }
}
