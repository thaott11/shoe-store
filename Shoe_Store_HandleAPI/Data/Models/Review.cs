using System.ComponentModel.DataAnnotations;

namespace Data.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Range(1, 5, ErrorMessage = "Point must be between 1 and 5.")]
        public int Point { get; set; }

        [MaxLength(500, ErrorMessage = "Comment cannot exceed 500 characters.")]
        public string Comment { get; set; }

        public int ClientId { get; set; }
        public virtual Client? client { get; set; }

        public int ProductId { get; set; }
        public virtual Product? product { get; set; }
    }
}
