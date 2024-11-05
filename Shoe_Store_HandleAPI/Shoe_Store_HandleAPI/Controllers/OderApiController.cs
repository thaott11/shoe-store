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
    public class OderApiController : ControllerBase
    {
        private readonly ModelDbContext _db;
        public OderApiController(ModelDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Authorize(Policy = "ClientPolicy")]

        public async Task<IActionResult> GetAll()
        {
            var Order = _db.Orders.ToListAsync();
            return Ok(Order);
        }

        [HttpPost]
        [Authorize(Policy = "ClientPolicy")]

        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            order.Date = DateTime.UtcNow;
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();  

            foreach (var detail in order.orderDetails)
            {
                detail.OrderId = order.Id;  
                _db.OrderDetails.Add(detail);
            }

            await _db.SaveChangesAsync();  

            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
        }



        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrderById(int id)
        {
            var order = await _db.Orders.Include(o => o.orderDetails).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }
            return order;
        }
    }
}
