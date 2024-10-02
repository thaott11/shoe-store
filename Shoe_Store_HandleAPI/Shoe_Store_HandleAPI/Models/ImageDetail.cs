﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shoe_Store_HandleAPI.Models
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
