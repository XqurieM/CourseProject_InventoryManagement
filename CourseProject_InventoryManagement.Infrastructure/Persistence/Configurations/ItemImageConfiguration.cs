using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourseProject_InventoryManagement.Infrastructure.Persistence.Configurations
{
    public sealed class ItemImageConfiguration : IEntityTypeConfiguration<ItemImage>
    {
        public void Configure(EntityTypeBuilder<ItemImage> builder)
        {
            builder.ToTable("ItemImages");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ImageUrl)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.Caption)
                .HasMaxLength(300);

            builder.Property(x => x.DisplayOrder)
                .IsRequired();

            builder.Property(x => x.IsPrimary)
                .IsRequired();

            builder.HasOne(x => x.Item)
                .WithMany(x => x.Images)
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.ItemId);
            builder.HasIndex(x => new { x.ItemId, x.DisplayOrder });
        }
    }
}
