using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Text.Json.Serialization;

namespace Data.Models
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }
        public decimal Price { get; set; }

        [MaxLength(3, ErrorMessage = "Size cannot exceed 3 characters.")]
        public string Size { get; set; }
        public int Quantity { get; set; }
        public int OrderId { get; set; }
        [JsonIgnore]
        public Order? order { get; set; }
        public int SanPhamId { get; set; }
        public Product? product { get; set; }
    }
}
