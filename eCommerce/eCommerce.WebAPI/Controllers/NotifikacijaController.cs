using eCommerce.Model.Requests;
using eCommerce.Model.Responses;
using eCommerce.Model.SearchObjects;
using eCommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.WebAPI.Controllers;

[Authorize]
public class NotifikacijaController : BaseCRUDController<NotifikacijaResponse, NotifikacijaSearchObject, NotifikacijaInsertRequest, NotifikacijaUpdateRequest, INotifikacijaService>
{
    public NotifikacijaController(INotifikacijaService service) : base(service)
    {
    }

    [HttpPost("{id}/OznaciProcitano")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotifikacijaResponse>> OznaciProcitano(int id)
    {
        var result = await _service.OznaciProcitanoAsync(id);
        return Ok(result);
    }
}
