using Data.VNPay;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shoe_Store_HandleAPI.Service.IVNPay;
using Microsoft.Extensions.Logging;

namespace Shoe_Store_HandleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;

        public PaymentController(IVnPayService vnPayService)
        {
            _vnPayService = vnPayService;
        }

        [HttpPost("CreatePaymentUrl")]
        public IActionResult CreatePaymentUrlVnpay([FromBody] PaymentInformationModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var paymentUrl = _vnPayService.CreatePaymentUrl(model, HttpContext);
                return Ok(new
                {
                    PaymentUrl = paymentUrl,
                    ReceivedData = model  
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        [HttpGet("PaymentCallback")]
        public IActionResult PaymentCallbackVnpay()
        {
            try
            {
                var queryParams = Request.Query;
                var response = _vnPayService.PaymentExecute(queryParams);

                if (response.Success)
                {
                    return Ok(new
                    {
                        Status = "Success",
                        Message = "Thanh toán thành công.",
                        Data = response
                    });
                }

                return BadRequest(new
                {
                    Status = "Failed",
                    Message = "Thanh toán thất bại.",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi: {ex.Message}");
            }
        }
    }
}
