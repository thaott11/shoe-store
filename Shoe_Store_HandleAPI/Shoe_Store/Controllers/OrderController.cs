using Data.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
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

        [HttpGet]
        public async Task<IActionResult> CreateOrder(int productId, string size)
        {
            var userTypeCookie = Request.Cookies["UserType"];
            var clientIdCookie = Request.Cookies["UserId"];

            var token = Request.Cookies["Shoe_Store_Cookie"];
            if (token == null)
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }

            if (productId <= 0)
            {
                ModelState.AddModelError(string.Empty, "Invalid product ID.");
                return View();
            }

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Shoe_Store_Cookie"]);

            var apiUrl = $"https://localhost:7172/api/products/{productId}";
            var response = await httpClient.GetAsync(apiUrl);
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return RedirectToAction("Forbidden", "Error");
            }
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
            if (!int.TryParse(clientIdCookie, out int clientId))
            {
                ModelState.AddModelError(string.Empty, "Invalid Client ID.");
                return View();
            }

            var order = new Order
            {
                ClientId = clientId, 
                OrderName = userTypeCookie,
                Total = product.Price,
                Date = DateTime.Now,
                orderDetails = new List<OrderDetail>
        {
            new OrderDetail
            {
                SanPhamId = productId,
                Price = product.Price,
                Size = size,
                product = product
            }
        }
            };

            ViewBag.ProductName = product.ProductName;
            ViewBag.Price = product.Price;
            ViewBag.ClientId = clientIdCookie;  
            ViewBag.OrderName = userTypeCookie;
            ViewBag.Size = size;
            ViewBag.ImageUrl = product.Image;

            return View(order);
        }


        [HttpPost]
        public async Task<IActionResult> CreateOrder(Order order, OrderDetail orderDetail)
        {
            var token = Request.Cookies["Shoe_Store_Cookie"];
            if (token == null)
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }


            var clientIdCookie = Request.Cookies["UserId"];
            if (clientIdCookie == null)
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }

            if (!int.TryParse(clientIdCookie, out int clientId))
            {
                ModelState.AddModelError(string.Empty, "Invalid Client ID.");
                return View();
            }
            order.ClientId = clientId;

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiOrderUrl = "https://localhost:7172/api/OderApi";
                var apiOrderDetailUrl = "https://localhost:7172/api/OrderDetailApi";

                var jsonOrder = JsonConvert.SerializeObject(order);
                var jsonOrderDetail = JsonConvert.SerializeObject(orderDetail);
                var contentOrder = new StringContent(jsonOrder, Encoding.UTF8, "application/json");
                var contentOrderDetail = new StringContent(jsonOrderDetail, Encoding.UTF8, "application/json");

                var responseOrder = await httpClient.PostAsync(apiOrderUrl, contentOrder);
                if (responseOrder.StatusCode == HttpStatusCode.Forbidden)
                {
                    return RedirectToAction("Forbidden", "Error");
                }
                if (responseOrder.IsSuccessStatusCode)
                {
                    var responseOrderDetail = await httpClient.PostAsync(apiOrderDetailUrl, contentOrderDetail);

                    if (responseOrderDetail.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Success");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to create order details.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to create order.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while creating the order.");
            }

            return View(order);
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
