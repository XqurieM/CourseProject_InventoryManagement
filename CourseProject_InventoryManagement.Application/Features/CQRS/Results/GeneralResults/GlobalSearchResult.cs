using CourseProject_InventoryManagement.Application.DTOs;
using System.Collections.Generic;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Results.GeneralResults
{
    public class GlobalSearchResult
    {
        public List<InventoryDto> Inventories { get; set; } = new List<InventoryDto>();
        public List<ItemDto> Items { get; set; } = new List<ItemDto>();
    }
}
