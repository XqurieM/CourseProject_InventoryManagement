using Ardalis.Result.AspNetCore;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.InventoryCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.InventoryQueries;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results.InventoryResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static CourseProject_InventoryManagement.Application.Features.CQRS.ICQRS;

namespace CourseProject_InventoryManagement.WebApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly ICreateInventory _createInventory;
        private readonly IGetInventoryById _getInventoryById;
        private readonly IAddInventoryField _addInventoryField;
        private readonly IAddInventoryCustomIdRules _addInventoryCustomIdRules;
        private readonly IUpdateInventoryAccess _updateInventoryAccess;
        private readonly IGetPopular5Inventories _getPopular5Inventories;
        private readonly IGetMyEditableInventories _getMyEditableInventories;
        private readonly IGetOwnInventories _getOwnInventories;
        private readonly IGetLast10Inventories _getLast10Inventories;
        private readonly IGetInventoryFieldsByInventoryId _getInventoryFieldsByInventoryId;
        private readonly IUpdateInventory _updateInventory;
        private readonly IDeleteInventory _deleteInventory;
        private readonly IGetInventoryAccessList _getInventoryAccessList;

        public InventoryController(ICreateInventory createInventory, IGetInventoryById getInventoryById, IAddInventoryField addInventoryField, IAddInventoryCustomIdRules addInventoryCustomIdRules, IUpdateInventoryAccess updateInventoryAccess, IGetPopular5Inventories getPopular5Inventories, IGetMyEditableInventories getMyEditableInventories, IGetOwnInventories getOwnInventories, IGetLast10Inventories getLast10Inventories, IGetInventoryFieldsByInventoryId getInventoryFieldsByInventoryId, IUpdateInventory updateInventory, IDeleteInventory deleteInventory, IGetInventoryAccessList getInventoryAccessList)
        {
            _createInventory = createInventory;
            _getInventoryById = getInventoryById;
            _addInventoryField = addInventoryField;
            _addInventoryCustomIdRules = addInventoryCustomIdRules;
            _updateInventoryAccess = updateInventoryAccess;
            _getPopular5Inventories = getPopular5Inventories;
            _getMyEditableInventories = getMyEditableInventories;
            _getOwnInventories = getOwnInventories;
            _getLast10Inventories = getLast10Inventories;
            _getInventoryFieldsByInventoryId = getInventoryFieldsByInventoryId;
            _updateInventory = updateInventory;
            _deleteInventory = deleteInventory;
            _getInventoryAccessList = getInventoryAccessList;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateInventory(CreateInventoryCommand command, CancellationToken cancellationToken)
        {
            var result = await _createInventory.CreateInventory(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<InventoryDto>> GetInventoryById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetInventoryByIdQuery { Id = id };
            var result = await _getInventoryById.GetInventoryById(query, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<GetInventoriesWithJoinInfosResult>>> GetPopular5Inventories(CancellationToken cancellationToken)
        {            
            var result = await _getPopular5Inventories.GetPopular5Inventories(cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<GetInventoriesWithJoinInfosResult>>> GetLast10Inventories(CancellationToken cancellationToken)
        {
            var result = await _getLast10Inventories.GetLast10Inventories(cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<GetProfileInventoriesResult>>> GetOwnInventories(Guid UserId,CancellationToken cancellationToken)
        {
            var result = await _getOwnInventories.GetOwnInventories(UserId,cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<GetProfileInventoriesResult>>> GetMyEditableInventories(Guid UserId, CancellationToken cancellationToken)
        {
            var result = await _getMyEditableInventories.GetMyEditableInventories(UserId,cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<GetInventoryFieldsByInventoryIdResult>>> GetInventoryFieldsByInventoryId(Guid inventoryId, CancellationToken cancellationToken)
        {
            var result = await _getInventoryFieldsByInventoryId.GetInventoryFieldsByInventoryId(inventoryId, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<InventoryAccessListDto>>> GetInventoryAccessList(GetInventoryAccessListQuery query, CancellationToken cancellationToken)
        {
            var result = await _getInventoryAccessList.GetInventoryAccessList(query.InventoryId, cancellationToken);
            return this.ToActionResult(result);
        }
            
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Guid>> AddInventoryField(AddInventoryFieldCommand command, CancellationToken cancellationToken)
        {
            var result = await _addInventoryField.AddInventoryField(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Guid>> AddInventoryCustomIdRules(AddInventoryCustomIdRulesCommand command, CancellationToken cancellationToken)
        {
            var result = await _addInventoryCustomIdRules.AddInventoryCustomIdRules(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Guid>> UpdateInventoryAccess(UpdateInventoryAccessCommand command, CancellationToken cancellationToken)
        {
            var result = await _updateInventoryAccess.UpdateInventoryAccess(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Guid>> UpdateInventory(UpdateInventoryCommand command, CancellationToken cancellationToken)
        {
            var result = await _updateInventory.UpdateInventory(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Guid>> DeleteInventory(DeleteInventoryCommand command, CancellationToken cancellationToken)
        {
            var result = await _deleteInventory.DeleteInventory(command, cancellationToken);
            return this.ToActionResult(result);
        }
    }
}
