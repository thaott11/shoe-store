using System.ComponentModel.DataAnnotations;

namespace Shoe_Store.Models
{
    public class LoginCallAPI
    {
        [Required(ErrorMessage = "Vui lòng nhập Email hoặc Tên đăng nhập")]
        public string UserMail { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string Passwords { get; set; }
    }
}
