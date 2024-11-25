using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Shoe_Store_HandleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OderApiController : ControllerBase
    {
        private readonly ModelDbContext _db;
        public OderApiController(ModelDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _db.Orders.ToListAsync();
            return Ok(orders);
        }

        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetOrderDetailById(int id)
        {
            var orderDetail = await _db.OrderDetails.FindAsync(id);
            if (orderDetail == null)
            {
                return NotFound("Order detail not found.");
            }
            return Ok(orderDetail);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrderById(int id)
        {
            var order = await _db.Orders.Include(o => o.orderDetails).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return Ok(JsonConvert.SerializeObject(order, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));
        }

        [HttpPost("createOrUpdate")]
        [Authorize(Policy = "ClientPolicy")]
        public async Task<IActionResult> CreateOrUpdateOrder([FromBody] OrderDetail newOrderDetail)
        {
            var clientIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (clientIdClaim == null || !int.TryParse(clientIdClaim.Value, out int clientId))
            {
                return Unauthorized("Không thể xác thực người dùng. Vui lòng đăng nhập.");
            }
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == newOrderDetail.SanPhamId);
            if (product == null)
            {
                return NotFound("Sản phẩm không tồn tại.");
            }
            if (newOrderDetail == null || newOrderDetail.Quantity <= 0 || newOrderDetail.Price <= 0 || newOrderDetail.SanPhamId <= 0)
            {
                return BadRequest("Thông tin sản phẩm không hợp lệ.");
            }
            if (product.Quantity < newOrderDetail.Quantity)
            {
                return BadRequest("Số lượng sản phẩm không đủ.");
            }
            try
            {
                var existingOrder = await _db.Orders.Include(o => o.orderDetails).FirstOrDefaultAsync(o => o.ClientId == clientId);
                if (existingOrder == null)
                {
                    var newOrder = new Order
                    {
                        OrderName = Request.Cookies["UserName"],
                        Total = newOrderDetail.Price * newOrderDetail.Quantity,
                        Date = DateTime.Now,
                        ClientId = clientId,
                        orderDetails = new List<OrderDetail>()
                    };
                    newOrder.orderDetails.Add(new OrderDetail
                    {
                        SanPhamId = newOrderDetail.SanPhamId,
                        Quantity = newOrderDetail.Quantity,
                        Price = newOrderDetail.Price,
                        Size = newOrderDetail.Size
                    });
                    product.Quantity -= newOrderDetail.Quantity;
                    _db.Orders.Add(newOrder);
                    await _db.SaveChangesAsync();

                    return CreatedAtAction(nameof(GetOrderById), new { id = newOrder.Id }, newOrder);
                }
                else
                {
                    var existingOrderDetail = existingOrder.orderDetails
                        .FirstOrDefault(od => od.SanPhamId == newOrderDetail.SanPhamId && od.Size == newOrderDetail.Size);

                    if (existingOrderDetail != null)
                    {
                        if (product.Quantity < newOrderDetail.Quantity)
                        {
                            return BadRequest("Số lượng sản phẩm không đủ.");
                        }
                        existingOrderDetail.Quantity += newOrderDetail.Quantity;
                        existingOrderDetail.Price = newOrderDetail.Price;
                        product.Quantity -= newOrderDetail.Quantity;
                    }
                    else
                    {
                        if (product.Quantity < newOrderDetail.Quantity)
                        {
                            return BadRequest($"Sản phẩm {product.ProductName} không đủ số lượng.");
                        }

                        existingOrder.orderDetails.Add(new OrderDetail
                        {
                            SanPhamId = newOrderDetail.SanPhamId,
                            Quantity = newOrderDetail.Quantity,
                            Price = newOrderDetail.Price,
                            Size = newOrderDetail.Size
                        });
                        product.Quantity -= newOrderDetail.Quantity;
                    }
                    existingOrder.Total += newOrderDetail.Price * newOrderDetail.Quantity;
                    try
                    {
                        await _db.SaveChangesAsync();
                    }
                    catch (DbUpdateException dbEx)
                    {
                        return StatusCode(500, $"Database update error: {dbEx.Message}");
                    }
                    return Ok(existingOrder);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("user/orders")]
        public async Task<IActionResult> GetOrdersByUser()
        {
            var clientIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (clientIdClaim == null || !int.TryParse(clientIdClaim.Value, out int clientId))
            {
                return Unauthorized("Không thể xác thực người dùng. Vui lòng đăng nhập.");
            }

            var clientExists = await _db.Clients.AnyAsync(c => c.Id == clientId);
            if (!clientExists)
            {
                return NotFound(new { message = "Client not found." });
            }

            var orders = await _db.Orders
                                  .Where(o => o.ClientId == clientId)
                                  .Include(o => o.orderDetails)
                                  .ThenInclude(od => od.product)
                                  .ToListAsync();

            if (orders == null || !orders.Any())
            {
                return NotFound(new { message = "No orders found for this client." });
            }

            // Log dữ liệu trả về để kiểm tra
            var ordersJson = JsonConvert.SerializeObject(orders, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            Console.WriteLine(ordersJson);
            return Ok(orders);
        }
    }

}
