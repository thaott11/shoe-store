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
    public class ProductSizeController : Controller
    {
        private readonly IHttpClientFactory _client;
        private readonly ILogger<ProductSizeController> _logger;

        public ProductSizeController(IHttpClientFactory client, ILogger<ProductSizeController> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<IActionResult> SizeList(int? page)
        {
            var token = Request.Cookies["Shoe_Store_Cookie"];
            if (token == null)
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }
            var client = _client.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Shoe_Store_Cookie"]);
            var url = "https://localhost:7172/api/ProductSize";
            var response = await client.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return RedirectToAction("Forbidden", "Error");
            }
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var productSizes = JsonConvert.DeserializeObject<List<ProductSize>>(jsonString);
                int pageSize = 5;
                int pageNumber = page ?? 1;
                var pageList = productSizes.ToPagedList(pageNumber, pageSize);
                return View(pageList);
            }
            else
            {
                return View(new List<ProductSize>().ToPagedList(1, 5));
            }
        }

        public IActionResult Create()
        {
            var token = Request.Cookies["Shoe_Store_Cookie"];
            if (token == null)
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductSize productSize)
        {
            var client = _client.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Shoe_Store_Cookie"]);
            var url = "https://localhost:7172/api/ProductSize";
            var jsonString = JsonConvert.SerializeObject(productSize);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return RedirectToAction("Forbidden", "Error");
            }
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("SizeList");
            }
            return NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            if (Request.Cookies["Shoe_Store_Cookie"] == null)
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }

            var client = _client.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Shoe_Store_Cookie"]);
            var url = $"https://localhost:7172/api/ProductSize/{id}";
            var response = await client.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return RedirectToAction("Forbidden", "Error");
            }
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var size = JsonConvert.DeserializeObject<ProductSize>(jsonString);
                return View(size);
            }
            return StatusCode((int)response.StatusCode, response.ReasonPhrase);
        }


        [HttpPost]
        public async Task<IActionResult> Update(ProductSize model)
        {

            var client = _client.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Shoe_Store_Cookie"]);
            var jsonConvert = JsonConvert.SerializeObject(model);
            var content = new StringContent(jsonConvert, Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"https://localhost:7172/api/ProductSize/{model.SizeId}", content);
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return RedirectToAction("Forbidden", "Error");
            }
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("SizeList");
            }
            return NotFound();
        }

        public async Task<IActionResult> Delete(int id)
        {
            var client = _client.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Shoe_Store_Cookie"]);
            var url = $"https://localhost:7172/api/ProductSize/{id}";
            var response = await client.DeleteAsync(url);
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return RedirectToAction("Forbidden", "Error");
            }
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("SizeList");
            }
            return View(response);
        }
    }
}
