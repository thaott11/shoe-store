using Data.DTO;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Shoe_Store_HandleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderDetailApiController : ControllerBase
    {
        public readonly ModelDbContext _db;
        public OrderDetailApiController(ModelDbContext db)
        {
            _db = db;
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "ClientPolicy")]
        public async Task<IActionResult> Delete(int id)
        {
            var orderDetail = await _db.OrderDetails.Include(od => od.order).Include(od => od.product).FirstOrDefaultAsync(od => od.Id == id);
            if (orderDetail == null)
            {
                return NotFound();
            }
            if (orderDetail.order != null)
            {
                orderDetail.order.Total -= orderDetail.Price * orderDetail.Quantity;
            }
            if (orderDetail.product != null)
            {
                orderDetail.product.Quantity += orderDetail.Quantity;
            }
            _db.OrderDetails.Remove(orderDetail);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                Message = "OrderDetail deleted successfully",
                UpdatedOrder = orderDetail.order,
                UpdatedProduct = orderDetail.product
            });
        }


       

        [HttpPut("{id}")]
        [Authorize(Policy = "ClientPolicy")]
        public async Task<ActionResult> UpdateQuantity(int id, [FromBody] OrderdetailUpdate request)
        {
            var existingDetail = await _db.OrderDetails
                .Include(od => od.order)
                .Include(od => od.product)
                .FirstOrDefaultAsync(od => od.Id == id);

            if (existingDetail == null)
            {
                return NotFound(new { Message = "OrderDetail không tồn tại!" });
            }

            if (request.NewQuantity <= 0)
            {
                return BadRequest(new { Message = "Số lượng phải lớn hơn 0!" });
            }

            var quantityDifference = request.NewQuantity - existingDetail.Quantity;

            if (existingDetail.order != null)
            {
                existingDetail.order.Total += quantityDifference * existingDetail.Price;
            }
            if (existingDetail.product != null)
            {
                existingDetail.product.Quantity -= quantityDifference;
                if (existingDetail.product.Quantity < 0)
                {
                    return BadRequest(new { Message = "Số lượng sản phẩm không đủ trong kho!" });
                }
            }
            existingDetail.Quantity = request.NewQuantity;
            await _db.SaveChangesAsync();

            return Ok("Cập nhật số lượng thành công!");
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDetail>> GetOrderDetailById(int id)
        {
            var orderDetail = await _db.OrderDetails.FindAsync(id);
            if (orderDetail == null)
            {
                return NotFound();
            }
            return orderDetail;
        }
    }
}
