using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Infrastructure.Configurations
{
    public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.ToTable("Comments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Content)
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .IsRequired();

            builder.HasOne(x => x.Inventory)
                .WithMany(x => x.Comments)
                .HasForeignKey(x => x.InventoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.InventoryId, x.CreatedAtUtc });
            builder.HasIndex(x => x.CreatedByUserId);
        }
    }
}
