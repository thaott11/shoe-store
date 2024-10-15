using Microsoft.AspNetCore.Mvc;
using Data.Models;
using System.Text.Json;
using System.Text;
using NuGet.Protocol.Plugins;
using System.Net;
using Data.Migrations;
using System.Net.Http.Headers;

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
            if (ModelState.IsValid)
            {
                var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
                var client = _client.CreateClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await client.PostAsync("https://localhost:7172/api/AuthMiddlewareAPI/LoginAPI", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<LoginResponse>(responseBody, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result != null && !string.IsNullOrEmpty(result.Token))
                    {
                        HttpContext.Session.SetString("JWTToken", result.Token);
                        HttpContext.Session.SetString("UserType", result.UserType);
                        HttpContext.Session.SetInt32("ClientId", result.ClientId);
                        if (result.UserType == "Client")
                        {
                            return RedirectToAction("ProductList", "User");
                        }
                        else if (result.UserType == "Admin")
                        {
                            return RedirectToAction("Dashboard", "Admin");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to retrieve token or user type from the login response.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login credentials.");
                }
            }

            return View(model);
        }



        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(Client client)
        {
            var json = JsonSerializer.Serialize(client);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Sử dụng HttpClient để gửi yêu cầu tới API đăng ký
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
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("JWTToken");
            return RedirectToAction("Login");
        }
    }
}


