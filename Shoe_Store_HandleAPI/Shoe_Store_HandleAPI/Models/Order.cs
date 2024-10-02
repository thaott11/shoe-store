using System.ComponentModel.DataAnnotations;

namespace Shoe_Store_HandleAPI.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CodeOrder { get; set; }
        [Required]
        [MaxLength(50, ErrorMessage = "Order name cannot exceed 50 characters.")]
        public string OrderName { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Total must be a positive value or zero.")]
        public decimal Total { get; set; }

        public DateTime Date { get; set; }

        public ICollection<OrderDetail> orderDetails { get; set; }

        public int ClientId { get; set; }
        public Client client { get; set; }
    }
}
