using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.GeneralQueries;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results.GeneralResults;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CourseProject_InventoryManagement.Application.Features.CQRS.ICQRS;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.GeneralHandlers
{
    public class GetDashboardStatisticsQueryHandler : ICQRS.IGetDashboardStatistics
    {
        private readonly IGetUsersById _getUsersById;
        private readonly IAppDbContext _context;

        public GetDashboardStatisticsQueryHandler(IGetUsersById getUsersById, IAppDbContext context)
        {
            _getUsersById = getUsersById;
            _context = context;
        }

        public async Task<Result<GetDashboardStatisticsResult>> GetDashboardStatistics(GetDashboardStatisticsQuery query, CancellationToken cancellationToken = default)
        {
            var GetCurrentUser = await _getUsersById.GetUsersById(new Queries.UserQueries.GetUserByIdQuery { Id = query.UserId }, cancellationToken);
            
            if (GetCurrentUser.Value.IsBlocked == true)
            {
                return Result<GetDashboardStatisticsResult>.Success(new GetDashboardStatisticsResult
                {
                    TotalInventoriesCount = -1,
                    TotalItemsCount = -1,
                    PublicInventoriesCount = -1,
                    ActiveContributorsCount = -1
                });
            }

            if (GetCurrentUser.Value.IsAdmin)
            {
                var InventoryCount = _context.Inventories.Count();
                var ItemsCount = _context.Items.Count();
                var publicInventoriesCount = _context.Inventories.Where(i => i.IsPublic).Count();
                var activeContributorsCount = _context.Users.Where(u => u.IsBlocked == false && u.IsDeleted == false).Count();
                return Result<GetDashboardStatisticsResult>.Success(new GetDashboardStatisticsResult
                {
                    TotalInventoriesCount = InventoryCount,
                    TotalItemsCount = ItemsCount,
                    PublicInventoriesCount = publicInventoriesCount,
                    ActiveContributorsCount = activeContributorsCount
                });
            }
            else
            {               
                var inventoryCount = _context.Inventories.Where(i => i.CreatedByUserId == GetCurrentUser.Value.Id).Count();
                var itemsCount = _context.Items.Where(i => i.CreatedByUserId == GetCurrentUser.Value.Id).Count();
                var publicInventoriesCount = _context.Inventories.Where(i => i.IsPublic).Count();
                var activeContributorsCount = 1;
                return Result<GetDashboardStatisticsResult>.Success(new GetDashboardStatisticsResult
                {
                    TotalInventoriesCount = inventoryCount,
                    TotalItemsCount = itemsCount,
                    PublicInventoriesCount = publicInventoriesCount,
                    ActiveContributorsCount = activeContributorsCount
                });
            }
        }   
    }
}
