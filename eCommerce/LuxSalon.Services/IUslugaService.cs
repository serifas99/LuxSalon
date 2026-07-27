using LuxSalon.Model.Requests;
using LuxSalon.Model.Responses;
using LuxSalon.Model.SearchObjects;

namespace LuxSalon.Services
{
    public interface IUslugaService : IBaseCRUDService<UslugaResponse, UslugaSearchObject, UslugaInsertRequest, UslugaUpdateRequest>
    {
    }
}
