using System.ComponentModel.DataAnnotations;

namespace Shoe_Store_HandleAPI.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public ICollection<Client> Clients { get; set; }
        public ICollection<Admin> Admins { get; set; }
    }
}
