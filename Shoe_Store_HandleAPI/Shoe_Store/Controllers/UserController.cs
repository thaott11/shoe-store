using Data.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PagedList;
using System.Net.Http;

namespace Shoe_Store.Controllers
{
    public class UserController : Controller
    {
        public readonly IHttpClientFactory httpClientFactory;
        public readonly HttpClient _client;
        public UserController(IHttpClientFactory httpClientFactory, HttpClient client)
        {
            this.httpClientFactory = httpClientFactory;
            this._client = client;
        }

        public async Task<IActionResult> ProductList()
        {
            var httpClient = httpClientFactory.CreateClient();
            var apiUrl = "https://localhost:7172/api/products";
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

        // Phương thức để lấy hình ảnh chính
        public async Task<IActionResult> GetImage(string imageName)
        {
            // Gọi API để lấy file ảnh dựa vào tên hình ảnh
            var response = await _client.GetAsync($"https://localhost:7172/api/Products/GetImage/{imageName}");

            if (response.IsSuccessStatusCode)
            {
                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/jpeg";
                return File(imageBytes, contentType); // Trả về hình ảnh dưới dạng FileContentResult
            }

            return NotFound(); // Trả về lỗi nếu không có ảnh
        }
        public async Task<IActionResult> GetImageDetail(string imageNamedetail)
        {
            var response = await _client.GetAsync($"https://localhost:7172/api/Products/GetImageDetail/{imageNamedetail}");
            if (response.IsSuccessStatusCode)
            {
                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                var contentType = response.Content.Headers.ContentType?.ToString() ?? "Imgdetail/jpeg";
                return File(imageBytes, contentType);
            }

            return NotFound();
        }


        public async Task<IActionResult> ProductDetail(int id)
        {
            var httpClient = httpClientFactory.CreateClient();

            // Lấy thông tin sản phẩm
            var apiUrl = $"https://localhost:7172/api/products/{id}";
            var response = await httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                return RedirectToAction("ProductList");
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var product = JsonConvert.DeserializeObject<Product>(jsonString);

            // Lấy tất cả category và productsize để hiển thị dưới dạng checkbox
            var categoryUrl = "https://localhost:7172/api/Category";
            var sizeUrl = "https://localhost:7172/api/ProductSize";

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
