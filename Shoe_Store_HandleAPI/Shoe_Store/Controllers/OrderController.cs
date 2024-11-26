using Data.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Shoe_Store.Controllers
{
    public class OrderController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public OrderController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(int ProductId, string SelectedSize)
        {
            var token = Request.Cookies["Shoe_Store_Cookie"];
            if (token == null)
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var apiUrl = $"https://localhost:7172/api/products/{ProductId}";
            var response = await httpClient.GetAsync(apiUrl);
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return RedirectToAction("Forbidden", "Error");
            }
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Error retrieving product information.");
                return View();
            }
            var productJson = await response.Content.ReadAsStringAsync();
            var product = JsonConvert.DeserializeObject<Product>(productJson);
            if (product == null || product.Quantity <= 0 || product.Price <= 0)
            {
                ModelState.AddModelError(string.Empty, "không có sản phẩm.");
                return View();
            }
            var orderDetail = new OrderDetail
            {
                SanPhamId = ProductId,
                Price = product.Price,
                Size = SelectedSize,
                Quantity = 1 
            };
            var jsonString = JsonConvert.SerializeObject(orderDetail);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var orderApiUrl = $"https://localhost:7172/api/OderApi/createOrUpdate";
            var orderResponse = await httpClient.PostAsync(orderApiUrl, content);

            if (orderResponse.IsSuccessStatusCode)
            {
                var responseContent = await orderResponse.Content.ReadAsStringAsync();
                var updatedOrder = JsonConvert.DeserializeObject<Order>(responseContent);
                return RedirectToAction("ListOrder", "User");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "lỗi nhé. không cập nhập được order.");
                return View();
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


        
    }
}
