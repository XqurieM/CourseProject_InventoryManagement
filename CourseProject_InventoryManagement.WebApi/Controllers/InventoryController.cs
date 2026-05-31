using Ardalis.Result.AspNetCore;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.InventoryCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.InventoryQueries;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results.InventoryResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static CourseProject_InventoryManagement.Application.Features.CQRS.ICQRS;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.WebApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly ICreateInventory _createInventory;
        private readonly IGetInventoryById _getInventoryById;
        private readonly IAddInventoryField _addInventoryField;
        private readonly IUpdateInventoryFields _updateInventoryFields;
        private readonly IDeleteInventoryField _deleteInventoryField;
        private readonly IReorderInventoryFields _reorderInventoryFields;
        private readonly IAddInventoryCustomIdRules _addInventoryCustomIdRules;
        private readonly IGetInventoryCustomIdRulesByInventoryId _getInventoryCustomIdRulesByInventoryId;
        private readonly IUpdateInventoryCustomIdRules _updateInventoryCustomIdRules;
        private readonly IDeleteInventoryCustomIdRule _deleteInventoryCustomIdRule;
        private readonly IReorderInventoryCustomIdRules _reorderInventoryCustomIdRules;
        private readonly IUpdateInventoryAccess _updateInventoryAccess;
        private readonly IUpdateInventoryTags _updateInventoryTags;
        private readonly IGetPopular5Inventories _getPopular5Inventories;
        private readonly IGetMyEditableInventories _getMyEditableInventories;
        private readonly IGetOwnInventories _getOwnInventories;
        private readonly IGetLast10Inventories _getLast10Inventories;
        private readonly IGetInventoryFieldsByInventoryId _getInventoryFieldsByInventoryId;
        private readonly IGetInventoryAccessList _getInventoryAccessList;
        private readonly IGetInventoryTagsByInventoryId _getInventoryTagsByInventoryId;
        private readonly IGetInventoryStatistics _getInventoryStatistics;
        private readonly IUpdateInventory _updateInventory;
        private readonly IDeleteInventory _deleteInventory;
        private readonly ISearchUsersForAccess _searchUsersForAccess;
        private readonly IGetOdooAggregatedResults _getOdooAggregatedResults;
        private readonly IGenerateInventoryApiToken _generateInventoryApiToken;
        private readonly ICreateOdooItems _createOdooItems;
        private readonly IDeleteOdooItem _deleteOdooItem;

        public InventoryController(
            ICreateInventory createInventory,
            IGetInventoryById getInventoryById,
            IAddInventoryField addInventoryField,
            IUpdateInventoryFields updateInventoryFields,
            IDeleteInventoryField deleteInventoryField,
            IReorderInventoryFields reorderInventoryFields,
            IAddInventoryCustomIdRules addInventoryCustomIdRules,
            IGetInventoryCustomIdRulesByInventoryId getInventoryCustomIdRulesByInventoryId,
            IUpdateInventoryCustomIdRules updateInventoryCustomIdRules,
            IDeleteInventoryCustomIdRule deleteInventoryCustomIdRule,
            IReorderInventoryCustomIdRules reorderInventoryCustomIdRules,
            IUpdateInventoryAccess updateInventoryAccess,
            IUpdateInventoryTags updateInventoryTags,
            IGetPopular5Inventories getPopular5Inventories,
            IGetMyEditableInventories getMyEditableInventories,
            IGetOwnInventories getOwnInventories,
            IGetLast10Inventories getLast10Inventories,
            IGetInventoryFieldsByInventoryId getInventoryFieldsByInventoryId,
            IGetInventoryAccessList getInventoryAccessList,
            IGetInventoryTagsByInventoryId getInventoryTagsByInventoryId,
            IGetInventoryStatistics getInventoryStatistics,
            IUpdateInventory updateInventory,
            IDeleteInventory deleteInventory,
            ISearchUsersForAccess searchUsersForAccess,
            IGetOdooAggregatedResults getOdooAggregatedResults,
            IGenerateInventoryApiToken generateInventoryApiToken,
            ICreateOdooItems createOdooItems,
            IDeleteOdooItem deleteOdooItem)
        {
            _createInventory = createInventory;
            _getInventoryById = getInventoryById;
            _addInventoryField = addInventoryField;
            _updateInventoryFields = updateInventoryFields;
            _deleteInventoryField = deleteInventoryField;
            _reorderInventoryFields = reorderInventoryFields;
            _addInventoryCustomIdRules = addInventoryCustomIdRules;
            _getInventoryCustomIdRulesByInventoryId = getInventoryCustomIdRulesByInventoryId;
            _updateInventoryCustomIdRules = updateInventoryCustomIdRules;
            _deleteInventoryCustomIdRule = deleteInventoryCustomIdRule;
            _reorderInventoryCustomIdRules = reorderInventoryCustomIdRules;
            _updateInventoryAccess = updateInventoryAccess;
            _updateInventoryTags = updateInventoryTags;
            _getPopular5Inventories = getPopular5Inventories;
            _getMyEditableInventories = getMyEditableInventories;
            _getOwnInventories = getOwnInventories;
            _getLast10Inventories = getLast10Inventories;
            _getInventoryFieldsByInventoryId = getInventoryFieldsByInventoryId;
            _getInventoryAccessList = getInventoryAccessList;
            _getInventoryTagsByInventoryId = getInventoryTagsByInventoryId;
            _getInventoryStatistics = getInventoryStatistics;
            _updateInventory = updateInventory;
            _deleteInventory = deleteInventory;
            _searchUsersForAccess = searchUsersForAccess;
            _getOdooAggregatedResults = getOdooAggregatedResults;
            _generateInventoryApiToken = generateInventoryApiToken;
            _createOdooItems = createOdooItems;
            _deleteOdooItem = deleteOdooItem;
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
            var result = await _getInventoryById.GetInventoryById(new GetInventoryByIdQuery { Id = id }, cancellationToken);
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
        public async Task<ActionResult<List<GetProfileInventoriesResult>>> GetOwnInventories(Guid userId, CancellationToken cancellationToken)
        {
            var result = await _getOwnInventories.GetOwnInventories(userId, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<GetProfileInventoriesResult>>> GetMyEditableInventories(Guid userId, CancellationToken cancellationToken)
        {
            var result = await _getMyEditableInventories.GetMyEditableInventories(userId, cancellationToken);
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
        public async Task<ActionResult<List<GetInventoryCustomIdRulesByInventoryIdResult>>> GetInventoryCustomIdRulesByInventoryId(Guid inventoryId, CancellationToken cancellationToken)
        {
            var result = await _getInventoryCustomIdRulesByInventoryId.GetInventoryCustomIdRulesByInventoryId(inventoryId, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<InventoryAccessListDto>>> GetInventoryAccessList([FromQuery] Guid inventoryId, CancellationToken cancellationToken)
        {
            var result = await _getInventoryAccessList.GetInventoryAccessList(inventoryId, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<TagDto>>> GetInventoryTagsByInventoryId(Guid inventoryId, CancellationToken cancellationToken)
        {
            var result = await _getInventoryTagsByInventoryId.GetInventoryTagsByInventoryId(inventoryId, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<GetInventoryStatisticsResult>> GetInventoryStatistics(Guid inventoryId, CancellationToken cancellationToken)
        {
            var result = await _getInventoryStatistics.GetInventoryStatistics(inventoryId, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<InventoryAccessUserLookupDto>>> SearchUsersForAccess([FromQuery] SearchUsersForAccessQuery query, CancellationToken cancellationToken)
        {
            var result = await _searchUsersForAccess.SearchUsersForAccess(query, cancellationToken);
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
        public async Task<ActionResult<Guid>> UpdateInventoryFields(UpdateInventoryFieldsCommand command, CancellationToken cancellationToken)
        {
            var result = await _updateInventoryFields.UpdateInventoryFields(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Guid>> DeleteInventoryField(DeleteInventoryFieldCommand command, CancellationToken cancellationToken)
        {
            var result = await _deleteInventoryField.DeleteInventoryField(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Guid>> ReorderInventoryFields(ReorderInventoryFieldsCommand command, CancellationToken cancellationToken)
        {
            var result = await _reorderInventoryFields.ReorderInventoryFields(command, cancellationToken);
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
        public async Task<ActionResult<Guid>> UpdateInventoryCustomIdRules(UpdateInventoryCustomIdRulesCommand command, CancellationToken cancellationToken)
        {
            var result = await _updateInventoryCustomIdRules.UpdateInventoryCustomIdRules(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Guid>> DeleteInventoryCustomIdRule(DeleteInventoryCustomIdRuleCommand command, CancellationToken cancellationToken)
        {
            var result = await _deleteInventoryCustomIdRule.DeleteInventoryCustomIdRule(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Guid>> ReorderInventoryCustomIdRules(ReorderInventoryCustomIdRulesCommand command, CancellationToken cancellationToken)
        {
            var result = await _reorderInventoryCustomIdRules.ReorderInventoryCustomIdRules(command, cancellationToken);
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
        public async Task<ActionResult<Guid>> UpdateInventoryTags(UpdateInventoryTagsCommand command, CancellationToken cancellationToken)
        {
            var result = await _updateInventoryTags.UpdateInventoryTags(command, cancellationToken);
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

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<OdooInventoryAggregateDto>> GetOdooAggregatedResults([FromQuery] string token, CancellationToken cancellationToken)
        {
            var result = await _getOdooAggregatedResults.GetOdooAggregatedResults(new GetOdooAggregatedResultsQuery { Token = token }, cancellationToken);
            return this.ToActionResult(result);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<List<Guid>>> CreateOdooItems(CreateOdooItemsCommand command, CancellationToken cancellationToken)
        {
            var result = await _createOdooItems.CreateOdooItems(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> DeleteOdooItem(DeleteOdooItemCommand command, CancellationToken cancellationToken)
        {
            var result = await _deleteOdooItem.DeleteOdooItem(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<string>> RestoreAllItems([FromServices] IAppDbContext context, CancellationToken cancellationToken)
        {
            var items = await context.Items.IgnoreQueryFilters().ToListAsync(cancellationToken);
            foreach (var item in items)
            {
                item.IsDeleted = false;
                item.DeletedAtUtc = null;
            }
            await context.SaveChangesAsync(cancellationToken);
            return Ok($"Successfully restored {items.Count} items in database!");
        }

        [Authorize]
        [HttpPost("{id}")]
        public async Task<ActionResult<string>> GenerateApiToken(Guid id, CancellationToken cancellationToken)
        {
            var result = await _generateInventoryApiToken.GenerateInventoryApiToken(new GenerateInventoryApiTokenCommand { InventoryId = id }, cancellationToken);
            return this.ToActionResult(result);
        }
    }
}
