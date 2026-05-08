using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CourseProject_InventoryManagement.Application.Abstractions.Persistence
{
    public interface IAppDbContext
    {
        DbSet<AppUser> Users { get; }
        DbSet<Inventory> Inventories { get; }
        DbSet<Category> Categories { get; }
        DbSet<Item> Items { get; }
        DbSet<ItemFieldValue> ItemFieldValues { get; }
        DbSet<InventoryField> InventoryFields { get; }
        DbSet<InventoryCustomIdRule> InventoryCustomIdRules { get; }
        DbSet<InventoryAccess> InventoryAccesses { get; }
        DbSet<RefreshToken> RefreshTokens { get; }
        DbSet<Tag> Tags { get; }
        DbSet<InventoryTag> InventoryTags { get; }
        DbSet<Comment> Comments { get; }
        DbSet<ItemLike> ItemLikes { get; }
        DbSet<LocalizationResources> LocalizationResources { get; }
        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
