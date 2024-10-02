using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Data.Models
{
    public class Client
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(50, ErrorMessage = "Name cannot exceed 50 characters.")]
        public string Name { get; set; }
        public string Address { get; set; }
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string Phone { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }
        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters.")]
        public string Password { get; set; }
        public int status { get; set; }
        public int RoleId { get; set; }
        public virtual Role? Role { get; set; }
        public virtual ICollection<Order>? orders { get; set; }
        public virtual ICollection<Review>? reviews { get; set; }
    }
}
