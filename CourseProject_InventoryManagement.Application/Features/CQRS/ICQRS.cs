using Ardalis.Result;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.InventoryCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.ItemCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.InventoryQueries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS
{
    public interface ICQRS
    {
        #region InventoryInterfaces
        public interface ICreateInventory
        {
            Task<Result<Guid>> CreateInventory(CreateInventoryCommand command, CancellationToken cancellationToken = default);
        }

        public interface IGetInventoryById
        {
            Task<Result<InventoryDto>> GetInventoryById(GetInventoryByIdQuery query, CancellationToken cancellationToken = default);
        }

        public interface IAddInventoryField
        {
            Task<Result<Guid>> AddInventoryField(AddInventoryFieldCommand command, CancellationToken cancellationToken = default);
        }
        #endregion

        #region ItemsInterfaces
        public interface IAddItem
        {
            Task<Result<Guid>> AddItem(AddItemCommand command, CancellationToken cancellationToken = default);
        }

        #endregion
    }
}
