using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Data.Models;

namespace Data.Config
{
    public class ImageDetailConfig : IEntityTypeConfiguration<ImageDetail>
    {
        public void Configure(EntityTypeBuilder<ImageDetail> builder)
        {
            builder.HasKey(i => i.Id);

            builder.HasOne(i => i.Product)
                .WithMany(p => p.ImageDetails)
                .HasForeignKey(i => i.ProductId);
        }
    }
}
