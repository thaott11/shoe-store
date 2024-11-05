using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.Models;
using Microsoft.AspNetCore.Authorization;

namespace Shoe_Store_HandleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductSizeController : ControllerBase
    {
        private readonly ModelDbContext _db;

        public ProductSizeController(ModelDbContext db)
        {
            _db = db;
        }
        [HttpGet("Client")]
        public async Task<IActionResult> GetAllClient()
        {
            var productSizes = await _db.ProductSizes.ToListAsync();
            return Ok(productSizes);
        }

        [HttpGet]
        [Authorize(Policy = "AdminPolicy")]

        public async Task<IActionResult> GetAll()
        {
            var productSizes = await _db.ProductSizes.ToListAsync();
            return Ok(productSizes);
        }

        [HttpPost]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<ActionResult<ProductSize>> Create([FromBody] ProductSize productSize)
        {
            if (productSize == null)
            {
                return BadRequest("lỗi cmnr");
            }

            _db.ProductSizes.Add(productSize);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAll), new { id = productSize.SizeId }, productSize);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductSize>> GetById(int id)
        {
            var size = await _db.ProductSizes.FindAsync(id);
            if (size == null)
            {
                return NotFound("lỗi cmnr");
            }
            return Ok(size);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<ActionResult<ProductSize>> Update(int id, [FromBody] ProductSize productSize)
        {
            var update = await _db.ProductSizes.FindAsync(id);
            if (update == null)
            {
                return NotFound();
            }

            update.Size = productSize.Size;
            await _db.SaveChangesAsync();
            return Ok(update);
        }


        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<ActionResult<ProductSize>> Delete(int id)
        {
            var delete = await _db.ProductSizes.FindAsync(id);
            if (delete == null)
            {
                return NotFound(); 
            }

            _db.ProductSizes.Remove(delete);
            await _db.SaveChangesAsync(); 
            return Ok(delete);
        }
    }
}
