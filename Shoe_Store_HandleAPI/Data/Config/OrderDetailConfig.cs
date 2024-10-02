using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Data.Models;

namespace Data.Config
{
    public class OrderDetailConfig : IEntityTypeConfiguration<OrderDetail>
    {
        public void Configure(EntityTypeBuilder<OrderDetail> builder)
        {
            builder.HasKey(od => od.Id);

            builder.HasOne(od => od.order)
                .WithMany(o => o.orderDetails)
                .HasForeignKey(od => od.OrderId);

            builder.HasOne(od => od.product)
                .WithMany(p => p.orderDetails)
                .HasForeignKey(od => od.SanPhamId);
        }
    }
}
