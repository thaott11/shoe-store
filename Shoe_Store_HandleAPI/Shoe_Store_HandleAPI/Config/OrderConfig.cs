using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shoe_Store_HandleAPI.Models;

namespace Shoe_Store_HandleAPI.Config
{
    public class OrderConfig : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.CodeOrder)
                .IsRequired();

            builder.Property(o => o.OrderName)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasMany(o => o.orderDetails)
                .WithOne(od => od.order)
                .HasForeignKey(od => od.OrderId);

            builder.HasOne(o => o.client)
                .WithMany(c => c.orders)
                .HasForeignKey(o => o.ClientId);
        }
    }
}
