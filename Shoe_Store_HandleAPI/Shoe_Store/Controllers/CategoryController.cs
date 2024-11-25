using Data.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PagedList;
using System.Net;
using System.Net.Http;
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
            var token = Request.Cookies["Shoe_Store_Cookie"];
            if (token == null)
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }

            var client = _client.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);


            var url = "https://localhost:7172/api/Category";
            var response = await client.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.Forbidden) 
            {
                return RedirectToAction("Forbidden", "Error"); 
            }
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var categories = JsonConvert.DeserializeObject<List<Category>>(jsonString);
                int pageSize = 5;
                int pageNumber = page ?? 1;
                var pageList = categories.ToPagedList(pageNumber, pageSize);
                return View(pageList);
            }
            
            return View(new List<Category>().ToPagedList(1, 5));
        }

        public IActionResult CreateCategory()
        {
            var token = Request.Cookies["Shoe_Store_Cookie"];
            if (token == null)
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            var client = _client.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Shoe_Store_Cookie"]);
            var url = "https://localhost:7172/api/Category";
            var jsonString = JsonConvert.SerializeObject(category);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return RedirectToAction("Forbidden", "Error");
            }
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("CategoryList");
            }
            return View(category);
        }


        [HttpGet]
        public async Task<IActionResult> UpdateCategory(int id)
        {

            var token = Request.Cookies["Shoe_Store_Cookie"];
            if (token == null)
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }
            var client = _client.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Shoe_Store_Cookie"]);
            var url = $"https://localhost:7172/api/Category/{id}";
            var response = await client.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return RedirectToAction("Forbidden", "Error");
            }
            if (response.IsSuccessStatusCode)
            {   
                var jsonString = await response.Content.ReadAsStringAsync();
                var category = JsonConvert.DeserializeObject<Category>(jsonString);
                return View(category);
            }
            else
            {
                return StatusCode((int)response.StatusCode, response.ReasonPhrase);
            }
        }

        public async Task<IActionResult> UpdateCategory(Category model)
        {
            var client = _client.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Shoe_Store_Cookie"]);
            var jsonconvert = JsonConvert.SerializeObject(model);
            var content = new StringContent(jsonconvert, Encoding.UTF8 , "application/json");
            var respones = await client.PutAsync($"https://localhost:7172/api/Category/{model.Id}", content);
            if (respones.StatusCode == HttpStatusCode.Forbidden)
            {
                return RedirectToAction("Forbidden", "Error");
            }
            if (respones.IsSuccessStatusCode)
            {
                return RedirectToAction("CategoryList");
            }
            return NotFound();
        }


        public async Task<IActionResult> DeleteCategory(int id)
        {
            var client = _client.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Shoe_Store_Cookie"]);
            var url = $"https://localhost:7172/api/Category/{id}";
            var respons = await client.DeleteAsync(url);
            if (respons.StatusCode == HttpStatusCode.Forbidden)
            {
                return RedirectToAction("Forbidden", "Error");
            }
            if (respons.IsSuccessStatusCode)
            {
                return RedirectToAction("CategoryList");
            }
            TempData["ErrorMessage"] = "Error deleting product: " + respons.ReasonPhrase;
            return View(respons);

        }


    }
}
