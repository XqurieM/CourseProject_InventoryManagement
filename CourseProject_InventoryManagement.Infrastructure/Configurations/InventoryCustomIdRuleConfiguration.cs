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
    public sealed class InventoryCustomIdRuleConfiguration : IEntityTypeConfiguration<InventoryCustomIdRule>
    {
        public void Configure(EntityTypeBuilder<InventoryCustomIdRule> builder)
        {
            builder.ToTable("InventoryCustomIdRules");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PartOrder)
                .IsRequired();

            builder.Property(x => x.PartType)
                .HasConversion<byte>()
                .IsRequired();

            builder.Property(x => x.Format)
                .HasMaxLength(200);

            builder.Property(x => x.StaticTextValue)
                .HasMaxLength(200);

            builder.HasOne(x => x.Inventory)
                .WithMany(x => x.CustomIdRules)
                .HasForeignKey(x => x.InventoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.InventoryId);
            builder.HasIndex(x => new { x.InventoryId, x.PartOrder });
        }
    }
}
