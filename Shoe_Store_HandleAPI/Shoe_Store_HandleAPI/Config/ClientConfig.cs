using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shoe_Store_HandleAPI.Models;

namespace Shoe_Store_HandleAPI.Config
{
    public class ClientConfig : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasMany(c => c.orders)
                .WithOne(o => o.client)
                .HasForeignKey(o => o.ClientId);

            builder.HasMany(c => c.reviews)
                .WithOne(r => r.client)
                .HasForeignKey(r => r.ClientId);

            builder.HasOne(c => c.Role)
                .WithMany(r => r.Clients)
                .HasForeignKey(c => c.RoleId);
        }
    }
}
