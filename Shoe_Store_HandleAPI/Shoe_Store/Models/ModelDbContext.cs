using Microsoft.EntityFrameworkCore;

namespace Shoe_Store.Models
{
    public class ModelDbContext: DbContext
    {
        public ModelDbContext()
        {

        }
        public ModelDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=MYDESTOP\\SQLEXPRESS01;Database=Shoe_Store;Trusted_Connection=True;TrustServerCertificate=True");
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<ImageDetail> ImageDetails { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductSize> ProductSizes { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Admin> admins { get; set; }
        public DbSet<LoginCallAPI> loginCalls { get; set; }
    }
}
