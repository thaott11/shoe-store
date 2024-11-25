using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Common;
using PagedList;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

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
                return RedirectToAction("ckjfhfikf");
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

        public async Task<int> GetOrderCountByClientId()
        {
            var token = Request.Cookies["Shoe_Store_Cookie"];
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var apiUrl = "https://localhost:7172/api/OderApi/ToltalOrder/Client";
            var response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var orderCount = JsonConvert.DeserializeObject<int>(jsonString);

                Console.WriteLine($"API returned order count: {orderCount}");
                return orderCount;
            }
            Console.WriteLine("Failed to get order count from API");
            return 0;
        }


        [HttpGet]
        public async Task<ActionResult> _Layout()
        {
            var clientIdCookie = Request.Cookies["UserId"];

            if (string.IsNullOrEmpty(clientIdCookie))
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }

            var token = Request.Cookies["Shoe_Store_Cookie"];
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            int orderDetailCount = 0;

            try
            {
                var response = await client.GetAsync("api/OderApi/ToltalOrder/Client");
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    orderDetailCount = int.Parse(responseString);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching order detail count: {ex.Message}");
            }

            ViewBag.OrderDetailCount = orderDetailCount;

            return View();
        }


        [HttpGet]
        public async Task<IActionResult> ListOrder()
        {
            var userName = Request.Cookies["UserName"];
            var token = Request.Cookies["Shoe_Store_Cookie"];
            var clientIdCookie = Request.Cookies["UserId"];
            if (clientIdCookie == null || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }
            if (!int.TryParse(clientIdCookie, out int clientId))
            {
                ModelState.AddModelError(string.Empty, "Invalid Client ID.");
                return View();
            }
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await httpClient.GetAsync($"https://localhost:7172/api/OderApi/user/orders");
                var jsonstring = await response.Content.ReadAsStringAsync();
                var content = new StringContent(jsonstring, Encoding.UTF8, "application/json");
                var orders = JsonConvert.DeserializeObject<List<Order>>(jsonstring);
                if (!response.IsSuccessStatusCode)
                {
                    return NotFound();
                }
                ViewBag.UserName = userName;
                return View(orders);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                return View();
            }
        }
    }
}
