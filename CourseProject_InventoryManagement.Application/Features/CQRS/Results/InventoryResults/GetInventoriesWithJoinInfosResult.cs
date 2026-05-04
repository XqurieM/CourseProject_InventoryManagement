using CourseProject_InventoryManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Results.InventoryResults
{
    public class GetInventoriesWithJoinInfosResult
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? ImageUrl { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string UserName { get; set; }
        public bool IsPublic { get; set; }
        public int ItemCount { get; set; }
        public List<Tag> Tags { get; set; }
    }
}
