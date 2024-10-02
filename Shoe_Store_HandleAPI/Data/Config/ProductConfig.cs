using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Data.Models;

namespace Data.Config
{
    public class ProductConfig : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.ProductName)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasMany(p => p.ImageDetails)
                .WithOne(i => i.Product)
                .HasForeignKey(i => i.ProductId);

            builder.HasMany(p => p.orderDetails)
                .WithOne(od => od.product)
                .HasForeignKey(od => od.SanPhamId);

            builder.HasMany(p => p.reviews)
                .WithOne(r => r.product)
                .HasForeignKey(r => r.ProductId);

            builder
            .HasMany(p => p.productSizes)
            .WithMany(ps => ps.products)
            .UsingEntity<Dictionary<string, object>>(
                "ProductProductSizes",
                j => j
                    .HasOne<ProductSize>()
                    .WithMany()
                    .HasForeignKey("SizeId"),
                j => j
                    .HasOne<Product>()
                    .WithMany()
                    .HasForeignKey("ProductId"),
                j =>
                {
                    j.HasKey("ProductId", "SizeId");
                });
        }
    }
}
