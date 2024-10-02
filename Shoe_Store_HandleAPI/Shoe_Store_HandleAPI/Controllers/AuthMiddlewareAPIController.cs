using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shoe_Store_HandleAPI.Models;
using Shoe_Store_HandleAPI.Service;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Shoe_Store_HandleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthMiddlewareAPIController : ControllerBase
    {
        private readonly EmailService _emailService;
        private readonly ModelDbContext _db;
        private readonly IConfiguration _config;

        public AuthMiddlewareAPIController(EmailService emailService, ModelDbContext db, IConfiguration config)
        {
            _emailService = emailService;
            _db = db;
            _config = config;
        }

        [HttpPost("registerAPI")]
        public async Task<IActionResult> RegisterClient([FromBody] Client client)
        {
            try
            {
                if (await _db.Clients.AnyAsync(c => c.Email == client.Email))
                {
                    return BadRequest(new { Error = "Email đã tồn tại" });
                }

                client.status = 1;
                client.Password = BCrypt.Net.BCrypt.HashPassword(client.Password);
                await _db.Clients.AddAsync(client);
                await _db.SaveChangesAsync();

                // Passing both client.Email and client.ClientId to SendEmail
                await _emailService.SendEmail(client.Email, client.Id); // Ensure client.ClientId exists and is the correct ID field

                var token = GenerateJwtToken(client.Email, "Client");

                return Ok(new
                {
                    Token = token,
                    UserType = "Client"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }


        [HttpPost("LoginAPI")]
        public async Task<IActionResult> Login([FromBody] ModelLogin login)
        {
            try
            {
                var client = await _db.Clients.FirstOrDefaultAsync(x => x.Email == login.UserOrMail);
                if (client != null && BCrypt.Net.BCrypt.Verify(login.Password, client.Password))
                {
                    var token = GenerateJwtToken(client.Email, "Client");

                    HttpContext.Session.SetString("UserType", "Client");
                    HttpContext.Session.SetString("UserEmail", client.Email);

                    return Ok(new { Token = token, UserType = "Client" });
                }

                var admin = await _db.admins.FirstOrDefaultAsync(x => x.UserName == login.UserOrMail);
                if (admin != null && admin.Password == login.Password)
                {
                    var token = GenerateJwtToken(admin.UserName, "Admin");

                    HttpContext.Session.SetString("UserType", "Admin");
                    HttpContext.Session.SetString("UserName", admin.UserName);

                    return Ok(new { Token = token, UserType = "Admin" });
                }

                return Unauthorized(new { Error = "Sai thông tin đăng nhập." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        private string GenerateJwtToken(string emailOrUsername, string userType)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, emailOrUsername),
                new Claim("UserType", userType),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet("checkSession")]
        public IActionResult CheckSession()
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != null)
            {
                return Ok(new { UserType = userType });
            }
            return Unauthorized(new { Error = "Chưa đăng nhập." });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { Message = "Đăng xuất thành công." });
        }
    }
}
