using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
