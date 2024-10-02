using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Data.Models;

namespace Data.Config
{
    public class ReviewConfig : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasKey(r => r.Id);

            builder.HasOne(r => r.client)
                .WithMany(c => c.reviews)
                .HasForeignKey(r => r.ClientId);

            builder.HasOne(r => r.product)
                .WithMany(p => p.reviews)
                .HasForeignKey(r => r.ProductId);
        }
    }
}
