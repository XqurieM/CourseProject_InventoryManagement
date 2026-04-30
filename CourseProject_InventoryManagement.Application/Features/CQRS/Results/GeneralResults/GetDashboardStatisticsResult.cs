using CourseProject_InventoryManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Results.GeneralResults
{
    public class GetDashboardStatisticsResult
    {
        public int TotalInventoriesCount { get; set; }
        public int TotalItemsCount { get; set; }
        public int PublicInventoriesCount { get; set; }
        public int ActiveContributorsCount { get; set; }
    }
}
