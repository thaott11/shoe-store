using Microsoft.AspNetCore.Mvc;
using Shoe_Store.Models;
using System.Text.Json;
using System.Text;

namespace Shoe_Store.Controllers
{
    public class AuthMiddlewareController : Controller
    {
        private readonly IHttpClientFactory _client;

        public AuthMiddlewareController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory;
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

        public IActionResult Login()
        {
            return View(new LoginCallAPI());
        }



        [HttpPost]
        public async Task<IActionResult> Login(LoginCallAPI model)
        {
            // Kiểm tra tính hợp lệ của model trước khi gửi yêu cầu đến API
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Tạo đối tượng dữ liệu đăng nhập
            var loginData = new { model.UserMail, Password = model.Passwords };
            var json = JsonSerializer.Serialize(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpClient = _client.CreateClient();
            var response = await httpClient.PostAsync("https://localhost:7172/api/AuthMiddlewareAPI/LoginAPI", content);

            // Kiểm tra nếu phản hồi từ API thành công
            if (response.IsSuccessStatusCode)
            {

                // Chuyển hướng đến Dashboard sau khi đăng nhập thành công
                return RedirectToAction("Dashboard", "Admin");

            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"Lỗi đăng nhập: {errorContent}");
            }

            // Nếu có lỗi, trả về view và hiển thị lỗi
            return View(model);
        }


        // Hàm gọi đăng xuất
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("JWTToken"); // Xóa token khỏi session
            return RedirectToAction("Login");
        }
    }
}
