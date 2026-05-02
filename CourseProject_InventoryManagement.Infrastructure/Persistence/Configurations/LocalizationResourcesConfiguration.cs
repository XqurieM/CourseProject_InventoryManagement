using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourseProject_InventoryManagement.Infrastructure.Persistence.Configurations
{
    public class LocalizationResourcesConfiguration : IEntityTypeConfiguration<LocalizationResources>
    {
        public void Configure(EntityTypeBuilder<LocalizationResources> builder)
        {
            builder.ToTable("LocalizationResources");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ResourceKey)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.LanguageCode)
                .IsRequired()
                .HasMaxLength(16);

            builder.Property(x => x.PageName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Value)
                .IsRequired()
                .HasMaxLength(4000);

            builder.HasIndex(x => new { x.PageName, x.LanguageCode, x.ResourceKey })
                .IsUnique();

            builder.HasIndex(x => new { x.LanguageCode, x.IsActive });
        }
    }
}
