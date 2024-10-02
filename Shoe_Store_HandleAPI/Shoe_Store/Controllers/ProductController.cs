using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PagedList;
using Shoe_Store.Models;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Shoe_Store.Controllers
{
    public class ProductController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProductController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> ProductList(int? page)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var apiUrl = "https://localhost:7172/api/products";
            var response = await httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var products = JsonConvert.DeserializeObject<List<Product>>(jsonString);

                int pageSize = 5;
                int pageNumber = (page ?? 1);
                var pagedList = products.ToPagedList(pageNumber, pageSize);

                return View(pagedList);
            }
            else
            {
                ViewBag.ErrorMessage = "Could not retrieve products from the API.";
                return View(new List<Product>().ToPagedList(1, 5));
            }
        }

        // Action hiển thị form tạo sản phẩm
        public async Task<IActionResult> CreateProduct()
        {
            // Lấy tất cả category và productsize để hiển thị dưới dạng checkbox
            var httpClient = _httpClientFactory.CreateClient();
            var categoryUrl = "https://localhost:7172/api/categories";
            var sizeUrl = "https://localhost:7172/api/productsizes";

            var categoryResponse = await httpClient.GetAsync(categoryUrl);
            var sizeResponse = await httpClient.GetAsync(sizeUrl);

            if (categoryResponse.IsSuccessStatusCode && sizeResponse.IsSuccessStatusCode)
            {
                var categoriesJson = await categoryResponse.Content.ReadAsStringAsync();
                var sizesJson = await sizeResponse.Content.ReadAsStringAsync();

                ViewBag.Categories = JsonConvert.DeserializeObject<List<Category>>(categoriesJson);
                ViewBag.ProductSizes = JsonConvert.DeserializeObject<List<ProductSize>>(sizesJson);

                return View();
            }
            else
            {
                ViewBag.ErrorMessage = "Could not retrieve categories or sizes from the API.";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(Product model, List<IFormFile> imageFiles, List<int> category, List<int> productsize)
        {
            if (ModelState.IsValid)
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiUrl = "https://localhost:7172/api/products";

                var formData = new MultipartFormDataContent();

                formData.Add(new StringContent(model.ProductName), "ProductName");
                formData.Add(new StringContent(model.Price.ToString()), "Price");
                formData.Add(new StringContent(model.Description), "Description");
                formData.Add(new StringContent(model.Quantity.ToString()), "Quantity");
                formData.Add(new StringContent(model.Color), "Color");

                // Gửi category
                foreach (var cat in category)
                {
                    formData.Add(new StringContent(cat.ToString()), "categorys");
                }

                // Gửi product size
                foreach (var size in productsize)
                {
                    formData.Add(new StringContent(size.ToString()), "productsize");
                }

                // Gửi các file hình ảnh
                if (imageFiles != null && imageFiles.Count > 0)
                {
                    foreach (var file in imageFiles)
                    {
                        var fileStreamContent = new StreamContent(file.OpenReadStream());
                        fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                        formData.Add(fileStreamContent, "imageFiles", file.FileName);
                    }
                }

                var response = await httpClient.PostAsync(apiUrl, formData);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("ProductList");
                }
                else
                {
                    ViewBag.ErrorMessage = "Error creating product.";
                }
            }
            return View(model);
        }

        // Action để hiển thị form update sản phẩm
        public async Task<IActionResult> UpdateProduct(int id)
        {
            var httpClient = _httpClientFactory.CreateClient();

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
            var categoryUrl = "https://localhost:7172/api/categories";
            var sizeUrl = "https://localhost:7172/api/productsizes";

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

        [HttpPost]
        public async Task<IActionResult> UpdateProduct(Product model, List<IFormFile> imageFiles, List<int> category, List<int> productsize)
        {
            if (ModelState.IsValid)
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiUrl = $"https://localhost:7172/api/products/{model.Id}";

                var formData = new MultipartFormDataContent();

                formData.Add(new StringContent(model.ProductName), "ProductName");
                formData.Add(new StringContent(model.Price.ToString()), "Price");
                formData.Add(new StringContent(model.Description), "Description");
                formData.Add(new StringContent(model.Quantity.ToString()), "Quantity");
                formData.Add(new StringContent(model.Color), "Color");

                // Gửi category đã chọn
                foreach (var cat in category)
                {
                    formData.Add(new StringContent(cat.ToString()), "categorys");
                }

                // Gửi product size đã chọn
                foreach (var size in productsize)
                {
                    formData.Add(new StringContent(size.ToString()), "productsize");
                }

                // Gửi các file hình ảnh
                if (imageFiles != null && imageFiles.Count > 0)
                {
                    foreach (var file in imageFiles)
                    {
                        var fileStreamContent = new StreamContent(file.OpenReadStream());
                        fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                        formData.Add(fileStreamContent, "imageFiles", file.FileName);
                    }
                }

                var response = await httpClient.PutAsync(apiUrl, formData);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("ProductList");
                }
                else
                {
                    ViewBag.ErrorMessage = "Error updating product.";
                }
            }

            return View(model);
        }

        public async Task<IActionResult> DeleteProduct(int id)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var apiUrl = $"https://localhost:7172/api/products/{id}";
            var response = await httpClient.DeleteAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("ProductList");
            }
            else
            {
                return RedirectToAction("ProductList");
            }
        }
    }
}
