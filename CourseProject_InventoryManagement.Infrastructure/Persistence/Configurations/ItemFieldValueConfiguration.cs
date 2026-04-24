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
    public sealed class ItemFieldValueConfiguration : IEntityTypeConfiguration<ItemFieldValue>
    {
        public void Configure(EntityTypeBuilder<ItemFieldValue> builder)
        {
            builder.ToTable("ItemFieldValues");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.StringValue);

            builder.Property(x => x.NumberValue)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.BooleanValue);

            builder.HasOne(x => x.Item)
                .WithMany(x => x.FieldValues)
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.InventoryField)
                .WithMany(x => x.ItemFieldValues)
                .HasForeignKey(x => x.InventoryFieldId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.ItemId, x.InventoryFieldId })
                .IsUnique();

            builder.HasIndex(x => x.InventoryFieldId);
        }
    }
}
