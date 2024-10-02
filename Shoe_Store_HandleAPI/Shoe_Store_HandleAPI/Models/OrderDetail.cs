using System.ComponentModel.DataAnnotations;

namespace Shoe_Store_HandleAPI.Models
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public int OrderId { get; set; }
        public Order order { get; set; }
        public int SanPhamId { get; set; }
        public Product product { get; set; }
    }
}
