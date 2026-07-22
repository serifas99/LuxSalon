using eCommerce.Model.Requests;
using eCommerce.Model.Responses;
using eCommerce.Model.SearchObjects;

namespace eCommerce.Services
{
    public interface ITerminService : IBaseCRUDService<TerminResponse, TerminSearchObject, TerminInsertRequest, TerminUpdateRequest>
    {
        Task<TerminResponse> PotvrdiAsync(int id);
        Task<TerminResponse> OtkaziAsync(int id);
        Task<TerminResponse> OznaciOdradjenAsync(int id);
        Task<TerminResponse> OznaciNijeSeOdazvaoAsync(int id);
        Task<List<string>> GetAllowedActionsAsync(int id);
    }
}
