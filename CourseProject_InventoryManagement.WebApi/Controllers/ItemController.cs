using Ardalis.Result.AspNetCore;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.ItemCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results.ItemResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static CourseProject_InventoryManagement.Application.Features.CQRS.ICQRS;

namespace CourseProject_InventoryManagement.WebApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly IAddItem _addItem;
        private readonly IAddItemFieldValues _addItemFieldValues;
        private readonly IGetItemsByInventoryId _getItemsByInventoryId;
        private readonly IGetItemFieldValuesByItemId _getItemFieldValuesByItemId;
        private readonly IGetItemFieldValuesByInventoryId _getItemFieldValuesByInventoryId;
        private readonly IUpdateItem _updateItem;

        public ItemController(IAddItem addItem, IAddItemFieldValues addItemFieldValues, IGetItemsByInventoryId getItemsByInventoryId, IGetItemFieldValuesByItemId getItemFieldValuesByItemId, IGetItemFieldValuesByInventoryId getItemFieldValuesByInventoryId, IUpdateItem updateItem)
        {
            _addItem = addItem;
            _addItemFieldValues = addItemFieldValues;
            _getItemsByInventoryId = getItemsByInventoryId;
            _getItemFieldValuesByItemId = getItemFieldValuesByItemId;
            _getItemFieldValuesByInventoryId = getItemFieldValuesByInventoryId;
            _updateItem = updateItem;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<List<Guid>>> AddItem(AddItemCommand command, CancellationToken cancellationToken)
        {
            var result = await _addItem.AddItem(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Guid>> AddItemFieldValues(AddItemFieldValuesCommand command, CancellationToken cancellationToken)
        {
            var result = await _addItemFieldValues.AddItemFieldValues(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Guid>> UpdateItem(UpdateItemCommand command, CancellationToken cancellationToken)
        {
            var result = await _updateItem.UpdateItem(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<ItemDto>>> GetItemsByInventoryId(Guid inventoryId, CancellationToken cancellationToken)
        {
            var result = await _getItemsByInventoryId.GetItemsByInventoryId(inventoryId, cancellationToken);
            return this.ToActionResult(result);    
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<ItemFieldValuesResult>>> GetItemFieldValuesByItemId(Guid itemId, CancellationToken cancellationToken)
        {
            var result = await _getItemFieldValuesByItemId.GetItemFieldValuesByItemId(itemId, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<ItemFieldValuesResult>>> GetItemFieldValuesByInventoryId(Guid inventoryId, CancellationToken cancellationToken)
        {
            var result = await _getItemFieldValuesByInventoryId.GetItemFieldValuesByInventoryId(inventoryId, cancellationToken);
            return this.ToActionResult(result);
        }
    }
}
