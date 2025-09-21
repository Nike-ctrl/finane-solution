using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Collections.Concurrent;
using Finanze.Web.Services;
using Finanze.Web.Models;

namespace Finanze.Web.Controllers
{
    [ApiController]
    //[Route("api/[controller]")]
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        private static string codeSend = string.Empty;

        private static DateTime lastSend = DateTime.Now;


        private readonly SenderService _senderService;

        public AccountController(SenderService senderService)
        {
            _senderService = senderService;
        }


        [HttpPost("sendcode")]
        public async Task<IActionResult> InviaCodiceAsync([FromQuery] string dt)
        {

            var codice = new Random().Next(10000000, 99999999).ToString();

            // Memorizza codice temporaneo (sovrascrive se già presente)
            codeSend = codice;
            lastSend = DateTime.Now;

            var replay = await _senderService.SendMessage(codice);          
            if (replay.IsSuccess)
            {
                return Ok(new { success = true });
            }
            else
            {
                return BadRequest(new { success = false, error = "Errore invio codice" });
            }

        }

        [HttpPost("login")]
        //public async Task<IActionResult> Login( [FromForm] string code)
        public async Task<IActionResult> Login([FromForm] LoginModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Code))
            {
                return Redirect("/login?error=1");
            }
            string code = model.Code;
            if (string.IsNullOrWhiteSpace(code))
                return Redirect("/login?error=1");

            if (string.IsNullOrWhiteSpace(codeSend))
                return Redirect("/login?notsend=1");

            double differenzaMinuti = (DateTime.Now - lastSend).TotalMinutes;
            if(differenzaMinuti > 2)
            {
                return Redirect("/login?exept=1");
            }

            if (codeSend != code)
            {
                return Redirect("/login?error=1");
            }

            // Codice corretto → autentica
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, "admin") };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Rimuove il codice usato
            codeSend = string.Empty;

            return Redirect("/");
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/login");
        }
    }
}
