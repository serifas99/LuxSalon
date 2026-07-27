using Azure;
using LuxSalon.Model.Access;
using LuxSalon.Model.Requests;
using LuxSalon.Services;
using LuxSalon.WebAPI.Services.AccessManager;
using Microsoft.AspNetCore.Mvc;

namespace LuxSalon.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccessController : Controller
    {
        private readonly IAccessManager _accessManager;
        private readonly IUserService _userService;

        public AccessController(IAccessManager accessManager, IUserService userService)
        {
            _accessManager = accessManager;
            _userService = userService;
        }

        [HttpPost("Login")]
        public async Task<ActionResult> Login([FromBody] UserLoginRequest request)
        {
            var result = await _accessManager.LoginAsync(request);
            return Ok(result);
        }

        [HttpPost("LoginWithRefreshToken")]
        public async Task<ActionResult> LoginWithRefreshToken([FromBody] RefreshAccessTokenRequest request)
        {
            var result = await _accessManager.LoginWithRefreshTokenAsync(request);
            return Ok(result);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UserInsertRequest request)
        {
            await _userService.InsertAsync(request);
            return Ok("You have registered successfully");
        }

        // "Zaboravljena lozinka" tok za mobilnu app: 1) ForgotPassword salje 6-cifreni kod na email
        // (preko RabbitMQ -> Subscriber -> MailHog), 2) ResetPassword provjerava kod i postavlja
        // novu lozinku. Namjerno anonimno (isti razlog kao Login/Register - korisnik po definiciji
        // nije prijavljen kad zaboravi lozinku), ali odgovor na ForgotPassword uvijek isti (200 OK)
        // bez obzira da li email postoji, da se ne otkriva koji emailovi su registrovani.
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            await _accessManager.ForgotPasswordAsync(request);
            return Ok(new { message = "Ako nalog sa ovim emailom postoji, kod za reset lozinke je poslan." });
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            await _accessManager.ResetPasswordAsync(request);
            return Ok(new { message = "Lozinka je uspješno promijenjena." });
        }
    }
}
