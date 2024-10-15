using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Data.Models;
using Shoe_Store_HandleAPI.Service;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

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


        [HttpGet("unauthorized")]
        public IActionResult Getunauthorized()
        {
            return Unauthorized();
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

                await _emailService.SendEmail(client.Email, client.Id);

                var token = GenerateJwtToken(client.Email, "Client", client.Id); // Thêm ID vào token

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
                // Tìm Client
                var client = await _db.Clients.FirstOrDefaultAsync(x => x.Email == login.UserOrMail);
                if (client != null && BCrypt.Net.BCrypt.Verify(login.Password, client.Password))
                {
                    // Tạo token cho Client
                    var token = GenerateJwtToken(client.Email, "Client", client.Id);

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, client.Email),
                        new Claim("UserType", "Client"),
                        new Claim("ClientId", client.Id.ToString()) // Lưu ClientId vào Claims
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    // Trả về thông tin UserType, Token và ClientId
                    return Ok(new { UserType = "Client", Token = token, ClientId = client.Id });
                }

                var admin = await _db.admins.FirstOrDefaultAsync(x => x.UserName == login.UserOrMail);
                if (admin != null && admin.Password == admin.Password)
                {
                    var token = GenerateJwtToken(admin.UserName, "Admin", admin.Id);

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, admin.UserName),
                        new Claim("UserType", "Admin"),
                        new Claim("UserId", admin.Id.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties
                    {
                        IsPersistent = true, // Tùy chọn lưu cookie lâu dài hoặc không
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                    });


                    return Ok(new { UserType = "Admin", Token = token });
                }

                return Unauthorized(new { Error = "Sai thông tin đăng nhập." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        private string GenerateJwtToken(string email, string userType, int userId)
        {
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim("UserType", userType),
            new Claim("UserId", userId.ToString())
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }



        [HttpGet("CheckLogin")]
        [Authorize]
        public IActionResult CheckLogin()
        {
            var userId = User.FindFirst("UserId")?.Value;
            var userType = User.FindFirst("UserType")?.Value;

            if (userId != null && userType != null)
            {
                return Ok(new { UserId = userId, UserType = userType });
            }

            return Unauthorized(new { Error = "User is not logged in." });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // Đăng xuất bằng cách xóa cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { Message = "Đăng xuất thành công." });
        }
    }
}