using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PagedList;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Shoe_Store.Controllers
{
    public class UserController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _client;

        public UserController(IHttpClientFactory httpClientFactory, HttpClient client)
        {
            _httpClientFactory = httpClientFactory;
            _client = client;
        }

        public async Task<IActionResult> ProductList()
        {
            var token = Request.Cookies["Shoe_Store_Cookie"];
            if (token == null)
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }

            var userTypeCookie = Request.Cookies["UserType"];
            var clientIdCookie = Request.Cookies["UserId"];

            if (userTypeCookie == null || clientIdCookie == null)
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }

            ViewData["message"] = $"Chào mừng {userTypeCookie} - ID Khách hàng: {clientIdCookie}";

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = "https://localhost:7172/api/products/client";
            var response = await httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var products = JsonConvert.DeserializeObject<List<Product>>(jsonString);

                return View(products);
            }
            else
            {
                ViewBag.ErrorMessage = "Could not retrieve products from the API.";
                return View(new List<Product>());
            }
        }


        public async Task<IActionResult> GetImage(string imageName)
        {
            var token = Request.Cookies["Shoe_Store_Cookie"];
            if (token == null)
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"https://localhost:7172/api/Products/GetImage/{imageName}");

            if (response.IsSuccessStatusCode)
            {
                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/jpeg";
                return File(imageBytes, contentType);
            }

            return NotFound();
        }

        public async Task<IActionResult> GetImageDetail(string imageNamedetail)
        {
            var token = Request.Cookies["Shoe_Store_Cookie"];
            if (token == null)
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"https://localhost:7172/api/Products/GetImageDetail/{imageNamedetail}");
            if (response.IsSuccessStatusCode)
            {
                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/jpeg";
                return File(imageBytes, contentType);
            }

            return NotFound();
        }

        public async Task<IActionResult> ProductDetail(int id)
        {
            if (Request.Cookies["Shoe_Store_Cookie"] == null)
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Shoe_Store_Cookie"]);

            var apiUrl = $"https://localhost:7172/api/products/{id}";
            var response = await httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                return RedirectToAction("ProductList");
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var product = JsonConvert.DeserializeObject<Product>(jsonString);
            var categoryUrl = "https://localhost:7172/api/Category/Client";
            var sizeUrl = "https://localhost:7172/api/ProductSize/Client";

            var categoryResponse = await httpClient.GetAsync(categoryUrl);
            var sizeResponse = await httpClient.GetAsync(sizeUrl);

            if (categoryResponse.IsSuccessStatusCode && sizeResponse.IsSuccessStatusCode)
            {
                var categoriesJson = await categoryResponse.Content.ReadAsStringAsync();
                var sizesJson = await sizeResponse.Content.ReadAsStringAsync();

                ViewBag.Categories = JsonConvert.DeserializeObject<List<Category>>(categoriesJson);
                ViewBag.ProductSizes = JsonConvert.DeserializeObject<List<ProductSize>>(sizesJson);
            }

            return View(product);
        }
    }
}
