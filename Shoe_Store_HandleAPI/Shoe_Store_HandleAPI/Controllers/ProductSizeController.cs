using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shoe_Store_HandleAPI.Models;

namespace Shoe_Store_HandleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductSizeController : ControllerBase
    {
        private readonly ModelDbContext _db;

        public ProductSizeController(ModelDbContext db)
        {
            _db = db;
        }

        // GET: api/ProductSize
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var productSizes = await _db.ProductSizes.ToListAsync();
            return Ok(productSizes);
        }

        // POST: api/ProductSize
        [HttpPost]
        public async Task<ActionResult<ProductSize>> Create([FromBody] ProductSize productSize)
        {
            if (productSize == null)
            {
                return BadRequest("ProductSize is null");
            }

            _db.ProductSizes.Add(productSize);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAll), productSize);
        }

        // PUT: api/ProductSize/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<ProductSize>> Update(int id, [FromBody] ProductSize productSize)
        {
            if (productSize == null)
            {
                return BadRequest("ProductSize is null");
            }

            var update = await _db.ProductSizes.FindAsync(id);
            if (update == null)
            {
                return NotFound(); // Trả về 404 nếu không tìm thấy product size
            }

            // Cập nhật các thuộc tính cần thiết
            update.Size = productSize.Size; // Giả sử có thuộc tính Size
            await _db.SaveChangesAsync(); // Đợi cho đến khi lưu thay đổi
            return Ok(update);
        }

        // DELETE: api/ProductSize/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<ProductSize>> Delete(int id)
        {
            var delete = await _db.ProductSizes.FindAsync(id);
            if (delete == null)
            {
                return NotFound(); // Trả về 404 nếu không tìm thấy product size
            }

            _db.ProductSizes.Remove(delete);
            await _db.SaveChangesAsync(); // Đợi cho đến khi lưu thay đổi
            return Ok(delete); // Trả về product size đã xóa
        }
    }
}
