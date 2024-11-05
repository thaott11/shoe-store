using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Data.Models;
using System.Security.Claims;

namespace Shoe_Store.Controllers
{
    public class VerifyGmailController : Controller
    {
        private readonly ModelDbContext _context;

        public VerifyGmailController(ModelDbContext context)
        {
            _context = context;
        }

        public IActionResult VerifyAccount()
        {
            return View();
        }

        [HttpGet("VerifyAccount/{id}")]
        public async Task<IActionResult> VerifyAccount(int id)
        {
            try
            {
                var client = await _context.Clients.FindAsync(id);

                if (client == null)
                {
                    return RedirectToAction("ErrorVetifyEmail", new { errorMessage = "Client not found." });
                }

                client.status = 2; 
                _context.Clients.Update(client);
                await _context.SaveChangesAsync();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, client.Id.ToString()),
                    new Claim(ClaimTypes.Email, client.Email),
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30) 
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                return RedirectToAction("ListProduct", "User");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", new { errorMessage = "An error occurred during verification. Please try again later." });
            }
        }

        public IActionResult ErrorPage(string errorMessage)
        {
            ViewData["ErrorMessage"] = errorMessage;
            return View();
        }
    }
}
