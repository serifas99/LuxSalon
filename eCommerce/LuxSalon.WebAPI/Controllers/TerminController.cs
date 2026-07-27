using LuxSalon.Model.Requests;
using LuxSalon.Model.Responses;
using LuxSalon.Model.SearchObjects;
using LuxSalon.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuxSalon.WebAPI.Controllers;

[Authorize]
public class TerminController : BaseCRUDController<TerminResponse, TerminSearchObject, TerminInsertRequest, TerminUpdateRequest, ITerminService>
{
    public TerminController(ITerminService service) : base(service)
    {
    }

    // Generic Update/Delete su namjerno onemoguceni - Termin ima state machine (vidi
    // TerminService) i mora se mijenjati iskljucivo preko Potvrdi/Otkazi/OznaciOdradjen/
    // OznaciNijeSeOdazvao akcija (koje imaju svoje provjere ovlascenja), nikad direktnim
    // prepisivanjem polja ili trajnim brisanjem zapisa o terminu.
    public override Task<ActionResult<TerminResponse>> Update(int id, [FromBody] TerminUpdateRequest request)
    {
        throw new LuxSalon.Model.Exceptions.ClinetException("Termin se ne moze direktno uređivati - koristite akcije za promjenu statusa.");
    }

    public override Task<IActionResult> Delete(int id)
    {
        throw new LuxSalon.Model.Exceptions.ClinetException("Termin se ne moze trajno obrisati - koristite otkazivanje.");
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

    [HttpGet("Dostupnost")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DostupnostDanaResponse>>> Dostupnost([FromQuery] int frizerId, [FromQuery] int uslugaId, [FromQuery] int godina, [FromQuery] int mjesec)
    {
        var result = await _service.DostupnostAsync(frizerId, uslugaId, godina, mjesec);
        return Ok(result);
    }

    [HttpGet("DostupniSlotovi")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<string>>> DostupniSlotovi([FromQuery] int frizerId, [FromQuery] int uslugaId, [FromQuery] DateTime datum)
    {
        var result = await _service.DostupniSlotoviAsync(frizerId, uslugaId, datum);
        return Ok(result);
    }
}
