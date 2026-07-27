using LuxSalon.Model.Requests;
using LuxSalon.Model.Responses;
using LuxSalon.Model.SearchObjects;

namespace LuxSalon.Services
{
    public interface IFrizerOcjenaService : IBaseCRUDService<FrizerOcjenaResponse, FrizerOcjenaSearchObject, FrizerOcjenaInsertRequest, FrizerOcjenaUpdateRequest>
    {
        Task<double> ProsjecnaOcjenaAsync(int frizerId);
    }
}
