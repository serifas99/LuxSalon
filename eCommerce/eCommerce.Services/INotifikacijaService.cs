using eCommerce.Model.Requests;
using eCommerce.Model.Responses;
using eCommerce.Model.SearchObjects;

namespace eCommerce.Services
{
    public interface INotifikacijaService : IBaseCRUDService<NotifikacijaResponse, NotifikacijaSearchObject, NotifikacijaInsertRequest, NotifikacijaUpdateRequest>
    {
        Task<NotifikacijaResponse> OznaciProcitanoAsync(int id);
    }
}
