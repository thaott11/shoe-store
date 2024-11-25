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
            var orderDetail = await _db.OrderDetails
                .Include(od => od.order)
                .Include(od => od.product)
                .FirstOrDefaultAsync(od => od.Id == id);

            if (orderDetail == null)
            {
                return NotFound();
            }

            // Cập nhật lại Order Total
            if (orderDetail.order != null)
            {
                orderDetail.order.Total -= orderDetail.Price * orderDetail.Quantity;
            }

            // Cập nhật lại Product Quantity
            if (orderDetail.product != null)
            {
                orderDetail.product.Quantity += orderDetail.Quantity;
            }

            // Xóa OrderDetail
            _db.OrderDetails.Remove(orderDetail);

            // Lưu thay đổi
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
        public async Task<ActionResult<OrderDetail>> Update(int id, [FromBody] OrderDetail updatedDetail)
        {
            var existingDetail = await _db.OrderDetails
                .Include(od => od.order)
                .Include(od => od.product)
                .FirstOrDefaultAsync(od => od.Id == id);

            if (existingDetail == null)
            {
                return NotFound();
            }

            // Tính sự thay đổi
            var quantityDifference = updatedDetail.Quantity - existingDetail.Quantity;
            var priceDifference = updatedDetail.Price - existingDetail.Price;

            // Cập nhật Total của Order
            if (existingDetail.order != null)
            {
                existingDetail.order.Total += (updatedDetail.Price * updatedDetail.Quantity) - (existingDetail.Price * existingDetail.Quantity);
            }

            // Cập nhật Quantity của Product
            if (existingDetail.product != null)
            {
                existingDetail.product.Quantity -= quantityDifference;
            }

            // Cập nhật thông tin OrderDetail
            existingDetail.Price = updatedDetail.Price;
            existingDetail.Quantity = updatedDetail.Quantity;
            existingDetail.Size = updatedDetail.Size;

            // Lưu thay đổi
            await _db.SaveChangesAsync();

            return Ok(new
            {
                Message = "OrderDetail updated successfully",
                UpdatedOrder = existingDetail.order,
                UpdatedProduct = existingDetail.product,
                UpdatedOrderDetail = existingDetail
            });
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
