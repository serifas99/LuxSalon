using LuxSalon.Model.Requests;
using LuxSalon.Model.Responses;
using LuxSalon.Model.SearchObjects;
using LuxSalon.Services;
using Microsoft.AspNetCore.Authorization;

namespace LuxSalon.WebAPI.Controllers;

[Authorize]
public class RadnoVrijemeController : BaseCRUDController<RadnoVrijemeResponse, RadnoVrijemeSearchObject, RadnoVrijemeInsertRequest, RadnoVrijemeUpdateRequest, IRadnoVrijemeService>
{
    public RadnoVrijemeController(IRadnoVrijemeService service) : base(service)
    {
    }
}
