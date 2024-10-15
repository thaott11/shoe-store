using Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

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

            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(products, options);
            return Ok(json);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> UpdateProduct(int id)
        {
            var product = await _db.Products.Include(p => p.ImageDetails).Include(p => p.Categories).Include(p => p.productSizes).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id,[FromForm] Product updatedProduct,[FromForm] List<int> categoryIds,[FromForm] List<int> productSizeIds,[FromForm] List<IFormFile> imageFiles)
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
                if (!string.IsNullOrEmpty(product.Image))
                {
                    var oldImagePath = Path.Combine(_env.WebRootPath, "images", product.Image);
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }
                product.Image = await SaveImageFile(updatedProduct.ImageFile, "images");
            }

            // Cập nhật category
            product.Categories.Clear();
            foreach (var categoryId in categoryIds)
            {
                var category = await _db.Categories.FindAsync(categoryId);
                if (category != null)
                    product.Categories.Add(category);
            }

            // Cập nhật size
            product.productSizes.Clear();
            foreach (var sizeId in productSizeIds)
            {
                var size = await _db.ProductSizes.FindAsync(sizeId);
                if (size != null)
                    product.productSizes.Add(size);
            }

            // capaj nhập img detail
            if (imageFiles != null && imageFiles.Count > 0)
            {
                // Xóa tất cả ảnh cũ
                foreach (var imageDetail in product.ImageDetails)
                {
                    var oldImagePath = Path.Combine(_env.WebRootPath, "Imgdetail", imageDetail.ImageUrl);
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }

                // Xóa ảnh phụ cũ khỏi cơ sở dữ liệu
                _db.ImageDetails.RemoveRange(product.ImageDetails);
                product.ImageDetails.Clear();

                // Thêm ảnh mới
                foreach (var imageFile in imageFiles)
                {
                    if (imageFile.Length > 0)
                    {
                        var imageUrl = await SaveImageFile(imageFile, "Imgdetail");
                        var imageDetail = new ImageDetail { ImageUrl = imageUrl, ProductId = product.Id };
                        product.ImageDetails.Add(imageDetail);
                        await _db.ImageDetails.AddAsync(imageDetail);
                    }
                }
            }
            else
            {
                // Nếu không có ảnh mới, xóa tất cả ảnh phụ
                foreach (var imageDetail in product.ImageDetails)
                {
                    var oldImagePath = Path.Combine(_env.WebRootPath, "Imgdetail", imageDetail.ImageUrl);
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }
                _db.ImageDetails.RemoveRange(product.ImageDetails);
                product.ImageDetails.Clear();
            }

            // Lưu tất cả thay đổi vào cơ sở dữ liệu
            await _db.SaveChangesAsync();
            return Ok(product);
        }



        [HttpPost]
        public async Task<ActionResult<Product>> AddProduct([FromForm] Product product, [FromForm] List<int> categorys,
    [FromForm] List<int> productsize, [FromForm] List<IFormFile> imageFiles)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Lưu ảnh chính
            if (product.ImageFile != null && product.ImageFile.Length > 0)
            {
                product.Image = await SaveImageFile(product.ImageFile, "images"); // Chỉ lưu tên file
            }

            // Thêm categories với product
            foreach (var categoryId in categorys)
            {
                var category = await _db.Categories.FindAsync(categoryId);
                if (category != null)
                {
                    product.Categories.Add(category);
                }
            }

            // Thêm product sizes với product
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

            // Lưu ảnh phụ
            if (imageFiles != null && imageFiles.Count > 0)
            {
                foreach (var imageFile in imageFiles)
                {
                    if (imageFile.Length > 0)
                    {
                        var imageUrl = await SaveImageFile(imageFile, "Imgdetail");
                        var imageDetail = new ImageDetail
                        {
                            ImageUrl = imageUrl, // Chỉ lưu tên file
                            ProductId = product.Id
                        };
                        await _db.ImageDetails.AddAsync(imageDetail);
                    }
                }
                await _db.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(Update), new { id = product.Id }, product);
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _db.Products.Include(p => p.ImageDetails).Include(p => p.Categories).Include(p => p.productSizes).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
                return NotFound();

            // Xóa ảnh chính
            if (!string.IsNullOrEmpty(product.Image))
            {
                var mainImagePath = Path.Combine(_env.WebRootPath, product.Image.TrimStart('/'));
                if (System.IO.File.Exists(mainImagePath))
                    System.IO.File.Delete(mainImagePath);
            }

            // Xóa ảnh phụ
            foreach (var imageDetail in product.ImageDetails)
            {
                var imagePath = Path.Combine(_env.WebRootPath, imageDetail.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }

            // Xóa các chi tiết hình ảnh và danh mục
            _db.ImageDetails.RemoveRange(product.ImageDetails);
            product.Categories.Clear();
            product.productSizes.Clear();

            // Xóa sản phẩm
            _db.Products.Remove(product);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private async Task<string> SaveImageFile(IFormFile imageFile, string folderName)
        {
            var imageDirectory = Path.Combine(_env.WebRootPath, folderName);

            if (!Directory.Exists(imageDirectory))
            {
                Directory.CreateDirectory(imageDirectory); 
            }
            var fileName = Path.GetRandomFileName() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(imageDirectory, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return fileName; 
        }



        [HttpGet("GetImage/{imageName}")]
        public async Task<IActionResult> GetImage(string imageName)
        {
            var wwwRootPath = _env.WebRootPath;
            var imagePath = Path.Combine(wwwRootPath, "images", imageName);

            if (!System.IO.File.Exists(imagePath))
            {
                return NotFound(); 
            }
            var imageFileStream = System.IO.File.OpenRead(imagePath);
            var mimeType = GetMimeType(imageName); 
            return new FileStreamResult(imageFileStream, mimeType);
        }

        [HttpGet("GetImageDetail/{imageName}")]
        public IActionResult GetImageDetail(string imageName)
        {
            var wwwRootPath = _env.WebRootPath;
            var imagePath = Path.Combine(wwwRootPath, "Imgdetail", imageName);

            if (!System.IO.File.Exists(imagePath))
            {
                return NotFound();
            }
            var imageFileStream = System.IO.File.OpenRead(imagePath);
            var mimeType = GetMimeType(imagePath);
            return new FileStreamResult(imageFileStream, mimeType);
        }

        private string GetMimeType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                _ => "application/octet-stream",
            };
        }



    }
}