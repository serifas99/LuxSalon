using LuxSalon.Model.Requests;
using LuxSalon.Model.Responses;
using LuxSalon.Model.SearchObjects;

namespace LuxSalon.Services
{
    public interface INotifikacijaService : IBaseCRUDService<NotifikacijaResponse, NotifikacijaSearchObject, NotifikacijaInsertRequest, NotifikacijaUpdateRequest>
    {
        Task<NotifikacijaResponse> OznaciProcitanoAsync(int id);
    }
}
