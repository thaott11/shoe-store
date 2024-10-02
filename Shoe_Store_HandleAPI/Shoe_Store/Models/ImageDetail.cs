using System.ComponentModel.DataAnnotations;

namespace Shoe_Store.Models
{
    public class ImageDetail
    {
        [Key]
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }
    }
}
