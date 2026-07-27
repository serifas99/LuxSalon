using LuxSalon.Model.Requests;
using LuxSalon.Model.Responses;
using LuxSalon.Model.SearchObjects;

namespace LuxSalon.Services
{
    public interface IObavijestService : IBaseCRUDService<ObavijestResponse, ObavijestSearchObject, ObavijestInsertRequest, ObavijestUpdateRequest>
    {
    }
}
