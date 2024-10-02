using System.ComponentModel.DataAnnotations;

namespace Shoe_Store_HandleAPI.Models
{
    public class ModelLogin
    {
        [Required]
        public string UserOrMail { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
