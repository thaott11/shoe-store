using CustomAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace handlerAPI.Models
{
    public class Roles
    {
        [Key]
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public ICollection<Client> Clients { get; set; } 
        public ICollection<Admin> Admins { get; set; }
    }
}
