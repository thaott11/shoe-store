using System.ComponentModel.DataAnnotations;

namespace Shoe_Store_HandleAPI.Models
{
    public class ProductSize
    {
        [Key]
        public int SizeId { get; set; }
        [Required]
        [MaxLength(3, ErrorMessage = "Size cannot exceed 3 characters.")]
        public string Size { get; set; }
        
        public virtual ICollection<Product>? products { get; set; } = new List<Product>();
    }
}
