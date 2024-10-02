﻿using System.ComponentModel.DataAnnotations;

namespace Shoe_Store.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(50, ErrorMessage = "Category name cannot exceed 50 characters.")]
        public string NameCategory { get; set; }
        public virtual ICollection<Product>? products { get; set; }
    }
}
