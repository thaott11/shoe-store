using Data.DTO;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Common;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Shoe_Store.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public PaymentController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment(string orderType, decimal total)
        {
            var token = Request.Cookies["Shoe_Store_Cookie"];
            if (token == null)
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }
            try
            {
                var paymentData = new
                {
                    orderType,
                    total,
                    orderCode = Guid.NewGuid().ToString(),
                    clientName = Request.Cookies["UserName"]
                };

                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var content = new StringContent(JsonConvert.SerializeObject(paymentData), Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://localhost:7172/api/Payment/CreatePaymentUrl", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var responseData = JsonConvert.DeserializeObject<PaymentResponse>(responseJson);

                    if (responseData == null || string.IsNullOrEmpty(responseData.PaymentUrl))
                    {
                        return BadRequest(new { message = "Lỗi: Không tìm thấy paymentUrl trong phản hồi từ API", responseJson });
                    }

                    return Redirect(responseData.PaymentUrl);
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return BadRequest(new { message = "Lỗi tạo thanh toán", error });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi trong quá trình xử lý", error = ex.Message });
            }
        }



        [HttpGet]
        public async Task<IActionResult> PaymentCallback()
        {
            try
            {
                // Đọc query parameters từ URL callback của VNPay
                var queryParams = Request.QueryString.Value;

                // Tạo một HttpClient từ IHttpClientFactory
                var client = _clientFactory.CreateClient();
                client.BaseAddress = new Uri("https://localhost:7172/");

                // Gửi dữ liệu callback đến API
                var response = await client.GetAsync($"api/Payment/PaymentCallback{queryParams}");

                if (response.IsSuccessStatusCode)
                {
                    var responseData = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

                    // Hiển thị trạng thái thành công
                    ViewBag.Message = responseData.Message;
                    ViewBag.Data = responseData.Data;

                    return View("PaymentSuccess");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();

                    // Hiển thị trạng thái thất bại
                    ViewBag.Message = "Thanh toán thất bại.";
                    ViewBag.Error = error;

                    return View("PaymentFailure");
                }
            }
            catch (Exception ex)
            {
                // Lỗi xử lý callback
                ViewBag.Message = "Đã xảy ra lỗi trong quá trình xử lý callback.";
                ViewBag.Error = ex.Message;
                return View("PaymentFailure");
            }
        }
    }
}
