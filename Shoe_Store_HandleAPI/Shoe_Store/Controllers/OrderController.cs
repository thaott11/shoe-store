using Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Shoe_Store.Controllers
{
    public class OrderController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public readonly HttpClient _client;


        public OrderController(IHttpClientFactory httpClientFactory, HttpClient client)
        {
            _httpClientFactory = httpClientFactory;
            _client = client;
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrder(int productId, string size)
        {

            var clientId = HttpContext.Session.GetInt32("ClientId");
            var nameClient = HttpContext.Session.GetString("UserType");

            if (clientId == null)
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }

            if (productId <= 0)
            {
                ModelState.AddModelError(string.Empty, "Invalid product ID.");
                return View();
            }

            var httpClient = _httpClientFactory.CreateClient();

            // Lấy thông tin sản phẩm
            var apiUrl = $"https://localhost:7172/api/products/{productId}";
            var response = await httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                return View();
            }

            var productJson = await response.Content.ReadAsStringAsync();
            var product = JsonConvert.DeserializeObject<Product>(productJson);
            if (product == null || product.Quantity <= 0 || product.Price <= 0)
            {
                ModelState.AddModelError(string.Empty, "Product information is invalid.");
                return View();
            }

            // Tạo một đơn hàng mới và chi tiết đơn hàng, bao gồm size
            var order = new Order
            {
                ClientId = clientId.Value,
                OrderName = nameClient,
                Total = product.Price,
                Date = DateTime.Now,
                orderDetails = new List<OrderDetail>
            {
                new OrderDetail
                {
                    SanPhamId = productId,
                    Quantity = 1,  // Giá trị mặc định cho số lượng
                    Price = product.Price,
                    Size = size,
                    product = product
                }
            }

            };

            // Chuẩn bị dữ liệu view cho phần xác nhận  
            ViewBag.ProductName = product.ProductName;
            ViewBag.Price = product.Price;
            ViewBag.ClientId = clientId;
            ViewBag.OrderName = nameClient;
            ViewBag.Size = size;
            ViewBag.ImageUrl = product.Image;

            return View(order);
        }

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




        [HttpPost]
        public async Task<IActionResult> CreateOrder(Order order, OrderDetail orderDetail)
        {
            // Lấy ClientId từ session
            var clientId = HttpContext.Session.GetInt32("ClientId");

            if (clientId == null)
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }

            // Set ClientId cho order
            order.ClientId = clientId.Value;

            try
            {
                // Tạo HttpClient để gọi API
                var httpClient = _httpClientFactory.CreateClient();

                // API endpoint URLs
                var apiOrderUrl = "https://localhost:7172/api/OderApi";
                var apiOrderDetailUrl = "https://localhost:7172/api/OrderDetailApi";

                // Serialize Order và OrderDetail thành JSON
                var jsonOrder = JsonConvert.SerializeObject(order);
                var jsonOrderDetail = JsonConvert.SerializeObject(orderDetail);

                // Tạo content cho POST request
                var contentOrder = new StringContent(jsonOrder, Encoding.UTF8, "application/json");
                var contentOrderDetail = new StringContent(jsonOrderDetail, Encoding.UTF8, "application/json");

                // Gửi yêu cầu POST tới API Order
                var responseOrder = await httpClient.PostAsync(apiOrderUrl, contentOrder);

                if (responseOrder.IsSuccessStatusCode)
                {
                    // Gửi yêu cầu POST tới API OrderDetail nếu Order thành công
                    var responseOrderDetail = await httpClient.PostAsync(apiOrderDetailUrl, contentOrderDetail);

                    if (responseOrderDetail.IsSuccessStatusCode)
                    {
                        // Nếu cả Order và OrderDetail đều thành công
                        return RedirectToAction("Success"); // Điều hướng sau khi hoàn tất
                    }
                    else
                    {
                        // Xử lý lỗi nếu OrderDetail không thành công
                        ModelState.AddModelError(string.Empty, "Failed to create order details.");
                    }
                }
                else
                {
                    // Xử lý lỗi nếu Order không thành công
                    ModelState.AddModelError(string.Empty, "Failed to create order.");
                }
            }
            catch
            {
                // Xử lý các lỗi ngoại lệ
                ModelState.AddModelError(string.Empty, "An error occurred while creating the order.");
            }

            return View(order); // Trả về View với dữ liệu hiện tại nếu có lỗi
        }
    }
}
