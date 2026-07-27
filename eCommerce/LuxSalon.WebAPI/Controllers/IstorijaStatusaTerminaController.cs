using LuxSalon.Model.Responses;
using LuxSalon.Model.SearchObjects;
using LuxSalon.Services;
using Microsoft.AspNetCore.Authorization;

namespace LuxSalon.WebAPI.Controllers;

[Authorize]
public class IstorijaStatusaTerminaController : BaseReadController<IstorijaStatusaTerminaResponse, IstorijaStatusaTerminaSearchObject, IIstorijaStatusaTerminaService>
{
    public IstorijaStatusaTerminaController(IIstorijaStatusaTerminaService service) : base(service)
    {
    }
}
