using LuxSalon.Model.Requests;
using LuxSalon.Model.Responses;
using LuxSalon.Model.SearchObjects;
using LuxSalon.Services;
using Microsoft.AspNetCore.Authorization;

namespace LuxSalon.WebAPI.Controllers;

[Authorize]
public class ObavijestController : BaseCRUDController<ObavijestResponse, ObavijestSearchObject, ObavijestInsertRequest, ObavijestUpdateRequest, IObavijestService>
{
    public ObavijestController(IObavijestService service) : base(service)
    {
    }
}
