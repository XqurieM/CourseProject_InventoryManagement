using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Infrastructure.Persistence.Configurations
{
    public sealed class ItemLikeConfiguration : IEntityTypeConfiguration<ItemLike>
    {
        public void Configure(EntityTypeBuilder<ItemLike> builder)
        {
            builder.ToTable("ItemLikes");

            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Item)
                .WithMany(x => x.Likes)
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.ItemId, x.CreatedByUserId })
                .IsUnique();

            builder.HasIndex(x => x.CreatedByUserId);
        }
    }
}
