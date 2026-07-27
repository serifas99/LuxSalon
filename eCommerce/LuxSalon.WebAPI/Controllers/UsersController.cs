using LuxSalon.Model.Access;
using LuxSalon.Model.Requests;
using LuxSalon.Model.Responses;
using LuxSalon.Model.SearchObjects;
using LuxSalon.Services;
using LuxSalon.WebAPI.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuxSalon.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class UsersController : BaseCRUDController<UserResponse, UserSearch, UserInsertRequest, UserUpdateRequest, IUserService>
{
    public UsersController(IUserService userService) : base(userService)
    {
    }

    public override Task<PageResult<UserResponse>> GetAll([FromQuery] UserSearch? search)
    {
        return base.GetAll(search);
    }

    [HttpPut("ChangePassword")]
    public async Task<IActionResult> ChangePassword([FromBody] UserPasswordChangeRequest request)
    {
        await _service.ChangePasswordAsync(request);
        return Ok();
    }

    [HttpGet("Klijenti")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PageResult<KlijentPregledResponse>>> Klijenti([FromQuery] UserSearch? search)
    {
        var result = await _service.GetKlijentiAsync(search ?? new UserSearch());
        return Ok(result);
    }
}