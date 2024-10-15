using Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using PagedList;
using System.Text;

namespace Shoe_Store.Controllers
{
    public class ProductSizeController : Controller
    {
        public readonly IHttpClientFactory _client;
        private readonly ILogger<ProductSizeController> _logger;
        public ProductSizeController(IHttpClientFactory client, ILogger<ProductSizeController> logger)
        {
            _client = client;
            _logger = logger; // Khởi tạo logger
        }

        public async Task<IActionResult> SizeList(int? page)
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }
            var client = _client.CreateClient();
            var url = "https://localhost:7172/api/ProductSize";
            var respons = await client.GetAsync(url);
            if (respons.IsSuccessStatusCode)
            {
                var jsonstring = await respons.Content.ReadAsStringAsync();
                var category = JsonConvert.DeserializeObject<List<ProductSize>>(jsonstring);
                int pagesize = 5;
                int pagecount = (page ?? 1);
                var pagelist = category.ToPagedList(pagecount, pagesize);
                return View(pagelist);
            }
            else
            {
                return View(new List<Category>().ToPagedList(1, 5));
            }
        }

        public IActionResult Create() 
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductSize productSize)
        {
            var client = _client.CreateClient();
            var url = "https://localhost:7172/api/ProductSize";
            var jsonString = JsonConvert.SerializeObject(productSize);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("SizeList");
            }
            return NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }
            var client = _client.CreateClient();
            var url = $"https://localhost:7172/api/ProductSize/{id}";
            var respons = await client.GetAsync(url);
            if(respons.IsSuccessStatusCode)
            {
                var jsonstring = await respons.Content.ReadAsStringAsync();
                var size = JsonConvert.DeserializeObject<ProductSize>(jsonstring);
                return View(size);
            }
            return StatusCode((int)respons.StatusCode, respons.ReasonPhrase);
        }

        [HttpPost]
        public async Task<IActionResult> Update(ProductSize model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); // Trả về view cùng lỗi nếu có
            }

            var client = _client.CreateClient();
            var url = $"https://localhost:7172/api/ProductSize/{model.SizeId}";
            var jsonContent = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var response = await client.PutAsync(url, jsonContent); 

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("SizeList"); 
            }
            else
            {
                return StatusCode((int)response.StatusCode, response.ReasonPhrase);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var client = _client.CreateClient();
            var url = $"https://localhost:7172/api/ProductSize/{id}";
            var response = await client.DeleteAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("SizeList");
            }
            return View(response);
        }
    }
}
