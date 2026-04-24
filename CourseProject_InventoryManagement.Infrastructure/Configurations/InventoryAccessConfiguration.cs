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
    public sealed class InventoryAccessConfiguration : IEntityTypeConfiguration<InventoryAccess>
    {
        public void Configure(EntityTypeBuilder<InventoryAccess> builder)
        {
            builder.ToTable("InventoryAccesses");

            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Inventory)
                .WithMany(x => x.Accesses)
                .HasForeignKey(x => x.InventoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.InventoryId, x.UserId })
                .IsUnique();

            builder.HasIndex(x => x.UserId);
        }
    }
}
