using System.ComponentModel.DataAnnotations;

namespace Data.Models
{
    public class ModelLogin
    {
        [Required]
        public string UserOrMail { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
