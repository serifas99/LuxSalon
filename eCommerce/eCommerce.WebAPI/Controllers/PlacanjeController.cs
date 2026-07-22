using eCommerce.Model.Responses;
using eCommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class PlacanjeController : ControllerBase
{
    private readonly IPlacanjeService _placanjeService;

    public PlacanjeController(IPlacanjeService placanjeService)
    {
        _placanjeService = placanjeService;
    }

    [HttpPost("Kreiraj/{terminId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PlacanjeKreirajResponse>> Kreiraj(int terminId)
    {
        var result = await _placanjeService.KreirajAsync(terminId);
        return Ok(result);
    }

    // AllowAnonymous - PayPal (ili klijent nakon odobravanja na sandbox stranici) poziva ovo bez JWT-a.
    [AllowAnonymous]
    [HttpPost("Potvrdi/{paypalOrderId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PlacanjeResponse>> Potvrdi(string paypalOrderId)
    {
        var result = await _placanjeService.PotvrdiAsync(paypalOrderId);
        return Ok(result);
    }

    [HttpPost("{id}/Vrati")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PlacanjeResponse>> Vrati(int id)
    {
        var result = await _placanjeService.VratiNovacAsync(id);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlacanjeResponse>> GetById(int id)
    {
        var result = await _placanjeService.GetByIdAsync(id);
        return Ok(result);
    }
}
