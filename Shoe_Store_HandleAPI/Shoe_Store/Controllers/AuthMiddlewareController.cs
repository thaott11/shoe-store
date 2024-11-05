using Microsoft.AspNetCore.Mvc;
using Data.Models;
using System.Text.Json;
using System.Text;
using System.Net.Http.Json;
using NuGet.Protocol.Plugins;
using System.Net;
using System.Net.Http.Headers;
using System.Linq;
using NuGet.Common;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net.Http;

namespace Shoe_Store.Controllers
{
    public class AuthMiddlewareController : Controller
    {
        private readonly IHttpClientFactory _client;

        public AuthMiddlewareController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory;
        }

        public IActionResult Login()
        {
            return View(new ModelLogin());
        }

        [HttpPost]
        public async Task<IActionResult> Login(ModelLogin model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:7172/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.PostAsJsonAsync("https://localhost:7172/api/AuthMiddlewareAPI/LoginAPI", model);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                if (result != null)
                {
                    var token = result.Token;
                    var userName = result.UserName;
                    var userType = result.UserType;
                    var userId = result.UserId;

                    Response.Cookies.Append("Shoe_Store_Cookie", token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddHours(1)
                    });
                    Response.Cookies.Append("UserType", userType, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddHours(1)
                    });
                    Response.Cookies.Append("UserId", userId.ToString(), new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddHours(1)
                    });

                    if (userType == "Client")
                    {
                        return RedirectToAction("ProductList", "User");
                    }
                    else if (userType == "Admin")
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }
                }
                ModelState.AddModelError(string.Empty, "Lỗi token");
                return View(model);
            }
            ModelState.AddModelError(string.Empty, "Lỗi cmnr.");
            return View(model);
        }


        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(Client client)
        {
            var json = JsonConvert.SerializeObject(client);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var httpClient = _client.CreateClient();
            var response = await httpClient.PostAsync("https://localhost:7172/api/AuthMiddlewareAPI/registerAPI", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Login");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"Lỗi đăng ký: {errorContent}");
                return View(client);
            }
        }
        public async Task<IActionResult> Logout()
        {
            var client = _client.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Shoe_Store_Cookie"]);
            var response = await client.PostAsync("https://localhost:7172/api/AuthMiddlewareAPI/logout", null);
            if (response.IsSuccessStatusCode)
            {
                Response.Cookies.Delete("Shoe_Store_Cookie");
                Response.Cookies.Delete("UserId");
                Response.Cookies.Delete("UserType");
                return RedirectToAction("Login");
            }
            else
            {
                ModelState.AddModelError("", "Failed to log out.");
                return NotFound();
            }
        }

        
    }
}


