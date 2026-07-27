using LuxSalon.Model.Requests;
using LuxSalon.Model.Responses;
using LuxSalon.Model.SearchObjects;
using LuxSalon.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuxSalon.WebAPI.Controllers;

[Authorize]
public class FrizerOcjenaController : BaseCRUDController<FrizerOcjenaResponse, FrizerOcjenaSearchObject, FrizerOcjenaInsertRequest, FrizerOcjenaUpdateRequest, IFrizerOcjenaService>
{
    public FrizerOcjenaController(IFrizerOcjenaService service) : base(service)
    {
    }

    [HttpGet("ProsjecnaOcjena/{frizerId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<double>> ProsjecnaOcjena(int frizerId)
    {
        var result = await _service.ProsjecnaOcjenaAsync(frizerId);
        return Ok(result);
    }
}
