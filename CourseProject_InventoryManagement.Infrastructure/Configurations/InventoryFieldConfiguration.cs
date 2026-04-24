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
    public sealed class InventoryFieldConfiguration : IEntityTypeConfiguration<InventoryField>
    {
        public void Configure(EntityTypeBuilder<InventoryField> builder)
        {
            builder.ToTable("InventoryFields");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.Property(x => x.FieldType)
                .HasConversion<byte>()
                .IsRequired();

            builder.Property(x => x.DisplayOrder)
                .IsRequired();

            builder.Property(x => x.IsRequired)
                .IsRequired();

            builder.Property(x => x.ShowInTable)
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .IsRequired();

            builder.HasOne(x => x.Inventory)
                .WithMany(x => x.Fields)
                .HasForeignKey(x => x.InventoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.InventoryId);
            builder.HasIndex(x => new { x.InventoryId, x.FieldType });
            builder.HasIndex(x => new { x.InventoryId, x.DisplayOrder });
        }
    }
}
