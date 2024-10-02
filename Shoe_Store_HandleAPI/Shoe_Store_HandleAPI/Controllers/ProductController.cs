using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Shoe_Store_HandleAPI.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Shoe_Store_HandleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ModelDbContext _db;
        private readonly IWebHostEnvironment _env;
        public ProductsController(ModelDbContext dbContext, IWebHostEnvironment webHostEnvironment)
        {
            _db = dbContext;
            _env = webHostEnvironment;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
        {
            var products = await _db.Products.Include(p => p.ImageDetails).Include(p => p.Categories).Include(p => p.productSizes).ToListAsync();
            return Ok(products);
        }


        [HttpPost]
        public async Task<ActionResult<Product>> AddProduct([FromForm] Product product, [FromForm] List<int> categorys,
        [FromForm] List<int> productsize, [FromForm] List<IFormFile> imageFiles)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (product.ImageFile != null && product.ImageFile.Length > 0)
            {
                product.Image = await SaveImageFile(product.ImageFile, "images"); 
            }

            // thêm categories với product
            foreach (var categoryId in categorys)
            {
                var category = await _db.Categories.FindAsync(categoryId);
                if (category != null)
                {
                    product.Categories.Add(category);
                }
            }

            // thêm product sizes với product
            foreach (var productsizess in productsize)
            {
                var pro = await _db.ProductSizes.FindAsync(productsizess);
                if (pro != null)
                {
                    product.productSizes.Add(pro);
                }
            }
            await _db.Products.AddAsync(product);
            await _db.SaveChangesAsync(); 

            if (imageFiles != null && imageFiles.Count > 0)
            {
                foreach (var imageFile in imageFiles)
                {
                    if (imageFile.Length > 0)
                    {
                        var imageUrl = await SaveImageFile(imageFile, "Imgdetail"); 
                        var imageDetail = new ImageDetail
                        {
                            ImageUrl = imageUrl,
                            ProductId = product.Id
                        };
                        await _db.ImageDetails.AddAsync(imageDetail);
                    }
                }
                await _db.SaveChangesAsync();
            }
            return CreatedAtAction(nameof(Update), new { id = product.Id }, product);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> Update(int id)
        {
            var product = await _db.Products.Include(p => p.ImageDetails).Include(p => p.Categories) .Include(p => p.productSizes).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }



        [HttpPut("{id}")]
        public async Task<ActionResult<Product>> Update(int id, [FromForm] Product updatedProduct, [FromForm] List<int> categorys,[FromForm] List<int> productsize, [FromForm] List<IFormFile> imageFiles)
        {
            var product = await _db.Products.Include(p => p.ImageDetails).Include(p => p.Categories).Include(p => p.productSizes).FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            product.ProductName = updatedProduct.ProductName;
            product.Price = updatedProduct.Price;
            product.Description = updatedProduct.Description;
            product.Quantity = updatedProduct.Quantity;
            product.Color = updatedProduct.Color;
            if (updatedProduct.ImageFile != null && updatedProduct.ImageFile.Length > 0)
            {
                product.Image = await SaveImageFile(updatedProduct.ImageFile, "images");
            }
            //cập nhật danh mục sản phẩm
            product.Categories.Clear();
            foreach (var categoryId in categorys)
            {
                var category = await _db.Categories.FindAsync(categoryId);
                if (category != null)
                {
                    product.Categories.Add(category);
                }
            }
            // cập nhật kích cỡ sản phẩm
            product.productSizes.Clear();
            foreach (var sizeId in productsize)
            {
                var size = await _db.ProductSizes.FindAsync(sizeId);
                if (size != null)
                {
                    product.productSizes.Add(size);
                }
            }

            // Cập nhật hình ảnh sản phẩm
            if (imageFiles != null && imageFiles.Count > 0)
            {
                _db.ImageDetails.RemoveRange(product.ImageDetails);
                foreach (var imageFile in imageFiles)
                {
                    if (imageFile.Length > 0)
                    {
                        var imageUrl = await SaveImageFile(imageFile, "Imgdetail");
                        var imageDetail = new ImageDetail
                        {
                            ImageUrl = imageUrl,
                            ProductId = product.Id
                        };
                        await _db.ImageDetails.AddAsync(imageDetail);
                    }
                }
            }
            await _db.SaveChangesAsync();
            return Ok(product);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _db.Products.Include(p => p.ImageDetails).Include(p => p.Categories).Include(p => p.productSizes).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
                return NotFound();
            foreach (var imageDetail in product.ImageDetails)
            {
                var imagePath = Path.Combine(_env.WebRootPath, imageDetail.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }
            _db.ImageDetails.RemoveRange(product.ImageDetails);
            product.Categories.Clear();
            product.productSizes.Clear();
            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // Phương thức lưu file ảnh vào thư mục 
        private async Task<string> SaveImageFile(IFormFile imageFile, string folderName)
        {
            var imageDirectory = Path.Combine(_env.WebRootPath, folderName);

            var fileName = Path.GetRandomFileName() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(imageDirectory, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }
            return $"/{folderName}/" + fileName;
        }
    }
}
