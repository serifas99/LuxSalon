using eCommerce.Model.Responses;
using eCommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.WebAPI.Controllers;

/// <summary>
/// Preporuke usluga za klijenta - hibridni Content-Based + Popularity-Based algoritam.
/// Vidi recommender-dokumentacija.md za detalje.
/// </summary>
[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public class RecommendationController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;

    public RecommendationController(IRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    [HttpGet("{klijentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UslugaPreporukaResponse>>> GetPreporuke(int klijentId, [FromQuery] int broj = 5)
    {
        var result = await _recommendationService.PreporuciAsync(klijentId, broj);
        return Ok(result);
    }
}
