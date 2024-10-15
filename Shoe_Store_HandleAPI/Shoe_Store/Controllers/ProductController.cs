using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PagedList;
using Data.Models;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Shoe_Store.Controllers
{
    public class ProductController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;

        public ProductController(IHttpClientFactory httpClientFactory, HttpClient client)
        {
            _httpClientFactory = httpClientFactory;
            _httpClient = client;
        }

        public async Task<IActionResult> ProductList(int? page)
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }
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


        public async Task<IActionResult> CreateProduct()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }
            var httpClient = _httpClientFactory.CreateClient();
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
                return View();
            }
            else
            {
                ViewBag.ErrorMessage = "Lỗi";
                return View();
            }
        }


        [HttpPost]
        public async Task<IActionResult> CreateProduct(Product model, List<IFormFile> imageFiles, List<string> category, List<string> productsize)
        {
            if (ModelState.IsValid)
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiUrl = "https://localhost:7172/api/products";

                using (var content = new MultipartFormDataContent())
                {
                    // Thêm ảnh chính
                    if (model.ImageFile != null)
                    {
                        var imageContent = new StreamContent(model.ImageFile.OpenReadStream());
                        imageContent.Headers.ContentType = new MediaTypeHeaderValue(model.ImageFile.ContentType);
                        content.Add(imageContent, "ImageFile", model.ImageFile.FileName);
                    }

                    // Thêm ảnh phụ
                    if (imageFiles != null)
                    {
                        foreach (var file in imageFiles)
                        {
                            if (file.Length > 0)
                            {
                                var fileContent = new StreamContent(file.OpenReadStream());
                                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                                content.Add(fileContent, "imageFiles", file.FileName); 
                            }
                        }
                    }

                    // Thêm thông tin sản phẩm
                    content.Add(new StringContent(model.ProductName), "ProductName");
                    content.Add(new StringContent(model.Price.ToString()), "Price");
                    content.Add(new StringContent(model.Description), "Description");
                    content.Add(new StringContent(model.Quantity.ToString()), "Quantity");
                    content.Add(new StringContent(model.Color), "Color");

                    // Thêm danh mục
                    if (category != null)
                    {
                        foreach (var categoryId in category)
                        {
                            content.Add(new StringContent(categoryId), "categorys"); // Thay đổi thành "categorys"
                        }
                    }

                    // Thêm kích thước sản phẩm
                    if (productsize != null)
                    {
                        foreach (var sizeId in productsize)
                        {
                            content.Add(new StringContent(sizeId), "productsize");
                        }
                    }

                    var response = await httpClient.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("ProductList");
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Lỗi";
                    }
                }
            }
            return View(model);
        }


        public async Task<IActionResult> UpdateProduct(int id)
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }
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


        [HttpPost]
        public async Task<IActionResult> UpdateProduct(Product model, List<IFormFile> imageFiles, List<int> category, List<int> productsize)
        {
            if (ModelState.IsValid)
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiUrl = $"https://localhost:7172/api/Products/{model.Id}";

                using (var formData = new MultipartFormDataContent())
                {
                    // Thêm thông tin sản phẩm
                    formData.Add(new StringContent(model.ProductName), "ProductName");
                    formData.Add(new StringContent(model.Price.ToString()), "Price");
                    formData.Add(new StringContent(model.Description), "Description");
                    formData.Add(new StringContent(model.Quantity.ToString()), "Quantity");
                    formData.Add(new StringContent(model.Color), "Color");

                    // Gửi category đã chọn
                    if (category != null && category.Count > 0)
                    {
                        foreach (var cat in category)
                        {
                            formData.Add(new StringContent(cat.ToString()), "categorys");
                        }
                    }

                    // Gửi product size đã chọn
                    if (productsize != null && productsize.Count > 0)
                    {
                        foreach (var size in productsize)
                        {
                            formData.Add(new StringContent(size.ToString()), "productsize");
                        }
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

                    // Gửi yêu cầu PUT đến API
                    var response = await httpClient.PutAsync(apiUrl, formData);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("ProductList");
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        ViewBag.ErrorMessage = $"Error updating product: {response.ReasonPhrase}. Details: {errorContent}";
                    }
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
                // Có thể thêm một thông báo thành công nếu cần thiết
                TempData["SuccessMessage"] = "Product deleted successfully.";
            }
            else
            {
                // Có thể thêm một thông báo lỗi nếu cần thiết
                TempData["ErrorMessage"] = "Error deleting product: " + response.ReasonPhrase;
            }

            return RedirectToAction("ProductList");
        }

        // Phương thức để lấy hình ảnh chính
        public async Task<IActionResult> GetImage(string imageName)
        {
            // Gọi API để lấy file ảnh dựa vào tên hình ảnh
            var response = await _httpClient.GetAsync($"https://localhost:7172/api/Products/GetImage/{imageName}");

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
            var response = await _httpClient.GetAsync($"https://localhost:7172/api/Products/GetImageDetail/{imageNamedetail}");
            if (response.IsSuccessStatusCode)
            {
                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                var contentType = response.Content.Headers.ContentType?.ToString() ?? "Imgdetail/jpeg";
                return File(imageBytes, contentType);
            }

            return NotFound();
        }
    }
}
