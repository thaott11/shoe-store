using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Shoe_Store.Models
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50, ErrorMessage = "Product name cannot exceed 50 characters.")]
        public string ProductName { get; set; }
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value or zero.")]
        public decimal Price { get; set; }
        [Required]
        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; }
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public int Quantity { get; set; }
        public string Color { get; set; }
        public string? Image { get; set; }

        [NotMapped]
        public IFormFile ImageFile { get; set; }
        public virtual ICollection<ImageDetail> ImageDetails { get; set; } = new List<ImageDetail>();

        public List<int> SelectedCategories { get; set; } = new List<int>();
        public virtual ICollection<Review>? reviews { get; set; }
        public virtual ICollection<OrderDetail>? orderDetails { get; set; }
        public List<int> SelectedProductSizes { get; set; } = new List<int>();
    }
}
