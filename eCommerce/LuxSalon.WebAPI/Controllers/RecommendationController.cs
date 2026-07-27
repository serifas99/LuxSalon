using LuxSalon.Model.Responses;
using LuxSalon.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuxSalon.WebAPI.Controllers;

/// <summary>
/// Preporuke usluga za klijenta - hibridni Content-Based + Popularity-Based algoritam.
/// Vidi recommender-dokumentacija.md za detalje.
/// </summary>
[ApiController]
[Route("[controller]")]
[Authorize]
public class RecommendationController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;
    private readonly IAuthenticatedUserAccessor _userAccessor;

    public RecommendationController(IRecommendationService recommendationService, IAuthenticatedUserAccessor userAccessor)
    {
        _recommendationService = recommendationService;
        _userAccessor = userAccessor;
    }

    // NAPOMENA: klijentId se NE prima iz URL-a (ranije je i endpoint bio AllowAnonymous,
    // pa je bilo ko mogao pogoditi tudji Id i vidjeti njegove personalizovane preporuke) -
    // uzima se iskljucivo iz JWT tokena prijavljenog korisnika.
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UslugaPreporukaResponse>>> GetPreporuke([FromQuery] int broj = 5)
    {
        var klijentId = _userAccessor.GetUserId();
        if (klijentId == null)
            return Unauthorized();

        var result = await _recommendationService.PreporuciAsync(klijentId.Value, broj);
        return Ok(result);
    }
}
