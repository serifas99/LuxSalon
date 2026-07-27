using System.Net;
using System.Text;
using LuxSalon.Model.Responses;
using LuxSalon.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuxSalon.WebAPI.Controllers;

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
        // Bazni URL nase WebAPI onako kako ga je browser na telefonu upravo pozvao (npr.
        // http://10.0.2.2:5126 za Android emulator) - PayPal treba ovo za return_url/cancel_url
        // "most" stranicu, vidi PayPalReturn/PayPalCancel niže i PayPalClient.cs.
        var backendBaseUrl = $"{Request.Scheme}://{Request.Host}";
        var result = await _placanjeService.KreirajAsync(terminId, backendBaseUrl);
        return Ok(result);
    }

    // Ove dvije rute su namjerno AllowAnonymous i NE vracaju JSON - PayPal-ova checkout stranica
    // (u browseru na telefonu) na njih radi obicnu HTTP redirekciju nakon sto korisnik odobri ili
    // otkaze placanje. Vracamo malu HTML stranicu koja odmah (JS + meta-refresh) preusmjerava na
    // nasu "luxsalon://payment/..." semu - to hvata mobilna app (AndroidManifest.xml intent-filter),
    // jer PayPal sam ne podrzava custom sheme direktno kao return_url/cancel_url.
    [AllowAnonymous]
    [HttpGet("PayPalReturn")]
    public ContentResult PayPalReturn([FromQuery] string? token, [FromQuery] string? PayerID)
    {
        var deepLink = $"luxsalon://payment/return?token={WebUtility.UrlEncode(token ?? string.Empty)}&PayerID={WebUtility.UrlEncode(PayerID ?? string.Empty)}";
        return Content(RedirectHtml(deepLink), "text/html", Encoding.UTF8);
    }

    [AllowAnonymous]
    [HttpGet("PayPalCancel")]
    public ContentResult PayPalCancel([FromQuery] string? token)
    {
        var deepLink = $"luxsalon://payment/cancel?token={WebUtility.UrlEncode(token ?? string.Empty)}";
        return Content(RedirectHtml(deepLink), "text/html", Encoding.UTF8);
    }

    private static string RedirectHtml(string deepLink) => $"""
        <!DOCTYPE html>
        <html>
        <head>
            <meta http-equiv="refresh" content="0;url={deepLink}" />
            <script>window.location.href = "{deepLink}";</script>
        </head>
        <body>
            <p>Vraćanje u LuxSalon aplikaciju... Ako se ne otvori automatski, <a href="{deepLink}">kliknite ovdje</a>.</p>
        </body>
        </html>
        """;

    // NAPOMENA: ovo NIJE AllowAnonymous. Iako korisnik odobrava placanje na eksternoj PayPal
    // stranici, potvrdu u nasoj aplikaciji i dalje salje mobilna app (PlacanjeProvider.potvrdi)
    // koja je cijelo vrijeme prijavljena i salje JWT - ownership se dodatno provjerava u servisu.
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
