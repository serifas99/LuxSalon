using eCommerce.Model.Requests;
using eCommerce.Model.Responses;
using eCommerce.Model.SearchObjects;
using eCommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.WebAPI.Controllers;

[Authorize]
public class TerminController : BaseCRUDController<TerminResponse, TerminSearchObject, TerminInsertRequest, TerminUpdateRequest, ITerminService>
{
    public TerminController(ITerminService service) : base(service)
    {
    }

    [HttpPost("{id}/Potvrdi")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TerminResponse>> Potvrdi(int id)
    {
        var result = await _service.PotvrdiAsync(id);
        return Ok(result);
    }

    [HttpPost("{id}/Otkazi")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TerminResponse>> Otkazi(int id)
    {
        var result = await _service.OtkaziAsync(id);
        return Ok(result);
    }

    [HttpPost("{id}/OznaciOdradjen")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TerminResponse>> OznaciOdradjen(int id)
    {
        var result = await _service.OznaciOdradjenAsync(id);
        return Ok(result);
    }

    [HttpPost("{id}/OznaciNijeSeOdazvao")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TerminResponse>> OznaciNijeSeOdazvao(int id)
    {
        var result = await _service.OznaciNijeSeOdazvaoAsync(id);
        return Ok(result);
    }

    [HttpGet("{id}/AllowedActions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<string>>> GetAllowedActions(int id)
    {
        var result = await _service.GetAllowedActionsAsync(id);
        return Ok(result);
    }
}
