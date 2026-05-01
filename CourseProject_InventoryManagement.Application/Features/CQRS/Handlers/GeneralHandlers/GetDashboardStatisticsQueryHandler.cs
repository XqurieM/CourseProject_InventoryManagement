using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.GeneralQueries;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results.GeneralResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
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
        private readonly ICurrentUserService _currentUserService;
        private readonly IAppDbContext _context;

        public GetDashboardStatisticsQueryHandler(ICurrentUserService currentUserService, IAppDbContext context)
        {
            _currentUserService = currentUserService;
            _context = context;
        }

        public async Task<Result<GetDashboardStatisticsResult>> GetDashboardStatistics(GetDashboardStatisticsQuery query, CancellationToken cancellationToken = default)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return Result<GetDashboardStatisticsResult>.Unauthorized("Authenticated user was not found.");
            }

            var currentUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == _currentUserService.UserId.Value && !x.IsDeleted, cancellationToken);

            if (currentUser is null)
            {
                return Result<GetDashboardStatisticsResult>.NotFound("Authenticated user was not found.");
            }

            if (currentUser.IsBlocked)
            {
                return Result<GetDashboardStatisticsResult>.Success(new GetDashboardStatisticsResult
                {
                    TotalInventoriesCount = -1,
                    TotalItemsCount = -1,
                    PublicInventoriesCount = -1,
                    ActiveContributorsCount = -1
                });
            }

            if (currentUser.IsAdmin)
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
                var inventoryCount = _context.Inventories.Where(i => i.CreatedByUserId == currentUser.Id).Count();
                var itemsCount = _context.Items.Where(i => i.CreatedByUserId == currentUser.Id).Count();
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
