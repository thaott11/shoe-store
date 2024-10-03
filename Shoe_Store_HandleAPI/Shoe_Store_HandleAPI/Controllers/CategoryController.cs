using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.Models;

namespace Shoe_Store_HandleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ModelDbContext _db;

        public CategoryController(ModelDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _db.Categories.ToListAsync();
            return Ok(categories); 
        }

        [HttpPost]
        public async Task<ActionResult<Category>> Create([FromBody] Category category)
        {
            if (category == null)
            {
                return BadRequest("Category is null");
            }

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAll), new { id = category.Id }, category);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetById(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound("Category not found");
            }
            return Ok(category);
        }


        [HttpPut("{id}")]
        public async Task<ActionResult<Category>> Update(int id, [FromBody] Category category)
        {
            var update = await _db.Categories.FindAsync(id);
            if (update == null)
            {
                return NotFound();
            }

            update.NameCategory = category.NameCategory;
            await _db.SaveChangesAsync();
            return Ok(update);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Category>> Delete(int id)
        {
            var delete = await _db.Categories.FindAsync(id);
            if (delete == null)
            {
                return NotFound(); 
            }

            _db.Categories.Remove(delete);
            await _db.SaveChangesAsync(); 
            return Ok(delete); 
        }
    }
}
