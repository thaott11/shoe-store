using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shoe_Store_HandleAPI.Models;

namespace Shoe_Store_HandleAPI.Config
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
