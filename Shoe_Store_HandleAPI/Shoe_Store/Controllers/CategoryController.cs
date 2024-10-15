using Data.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PagedList;
using System.Net.Http.Headers;
using System.Text;

namespace Shoe_Store.Controllers
{
    public class CategoryController : Controller
    {
        public readonly IHttpClientFactory _client;
        public CategoryController(IHttpClientFactory client)
        {
             _client = client;
        }

        public async Task<IActionResult> CategoryList(int? page)
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }

            var client = _client.CreateClient();
            var url = "https://localhost:7172/api/Category";
            var respons = await client.GetAsync(url);
            if (respons.IsSuccessStatusCode)
            {
                var jsonstring = await respons.Content.ReadAsStringAsync();
                var category = JsonConvert.DeserializeObject<List<Category>>(jsonstring);
                int pagesize = 5;
                int pagecount = (page ?? 1);
                var pagelist = category.ToPagedList(pagecount, pagesize);
                return View(pagelist);
            }

            return View(new List<Category>().ToPagedList(1, 5));
        }


        public IActionResult CreateCategory()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWTToken")))
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            var client = _client.CreateClient();
            var url = "https://localhost:7172/api/Category";
            var jsonString = JsonConvert.SerializeObject(category);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("CategoryList");
            }
            return View(category);
        }


        // GET: Category/UpdateCategory/5
        [HttpGet]
        public async Task<IActionResult> UpdateCategory(int id)
        {

            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }
            // Lấy thông tin category bằng id
            var client = _client.CreateClient();
            var url = $"https://localhost:7172/api/Category/{id}";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var category = JsonConvert.DeserializeObject<Category>(jsonString);
                return View(category); // Trả về view UpdateCategory cùng dữ liệu
            }
            else
            {
                return StatusCode((int)response.StatusCode, response.ReasonPhrase);
            }
        }

        // POST: Category/UpdateCategory
        [HttpPost]
        public async Task<IActionResult> UpdateCategory(Category model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); // Trả về view cùng lỗi nếu có
            }

            var client = _client.CreateClient();
            var url = $"https://localhost:7172/api/Category/{model.Id}";
            var jsonContent = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var response = await client.PutAsync(url, jsonContent); // Gửi PUT request để cập nhật category

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("CategoryList"); // Chuyển hướng sau khi cập nhật thành công
            }
            else
            {
                return StatusCode((int)response.StatusCode, response.ReasonPhrase);
            }
        }


        public async Task<IActionResult> DeleteCategory(int id)
        {
            var client = _client.CreateClient();
            var url = $"https://localhost:7172/api/Category/{id}";
            var respons = await client.DeleteAsync(url);
            if (respons.IsSuccessStatusCode)
            {
                return RedirectToAction("CategoryList");
            }
            TempData["ErrorMessage"] = "Error deleting product: " + respons.ReasonPhrase;
            return View(respons);

        }
    }
}
