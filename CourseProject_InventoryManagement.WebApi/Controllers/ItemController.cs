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
        private readonly IDeleteItem _deleteItem;
        private readonly IGetItemById _getItemById;
        private readonly IGetItemImagesByItemId _getItemImagesByItemId;
        private readonly IToggleItemLike _toggleItemLike;
        private readonly IUpdateItemFieldValues _updateItemFieldValues;
        private readonly IUpdateItemImages _updateItemImages;

        public ItemController(IAddItem addItem, IAddItemFieldValues addItemFieldValues, IGetItemsByInventoryId getItemsByInventoryId, IGetItemFieldValuesByItemId getItemFieldValuesByItemId, IGetItemFieldValuesByInventoryId getItemFieldValuesByInventoryId, IUpdateItem updateItem, IDeleteItem deleteItem, IGetItemById getItemById, IGetItemImagesByItemId getItemImagesByItemId, IToggleItemLike toggleItemLike, IUpdateItemFieldValues updateItemFieldValues, IUpdateItemImages updateItemImages)
        {
            _addItem = addItem;
            _addItemFieldValues = addItemFieldValues;
            _getItemsByInventoryId = getItemsByInventoryId;
            _getItemFieldValuesByItemId = getItemFieldValuesByItemId;
            _getItemFieldValuesByInventoryId = getItemFieldValuesByInventoryId;
            _updateItem = updateItem;
            _deleteItem = deleteItem;
            _getItemById = getItemById;
            _getItemImagesByItemId = getItemImagesByItemId;
            _toggleItemLike = toggleItemLike;
            _updateItemFieldValues = updateItemFieldValues;
            _updateItemImages = updateItemImages;
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
        public async Task<ActionResult<Guid>> UpdateItemFieldValues(UpdateItemFieldValuesCommand command, CancellationToken cancellationToken)
        {
            var result = await _updateItemFieldValues.UpdateItemFieldValues(command, cancellationToken);
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
        [HttpPost]
        public async Task<ActionResult<Guid>> DeleteItem(DeleteItemCommand command, CancellationToken cancellationToken)
        {
            var result = await _deleteItem.DeleteItem(command, cancellationToken);
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
        public async Task<ActionResult<ItemDto>> GetItemById(Guid itemId, CancellationToken cancellationToken)
        {
            var result = await _getItemById.GetItemById(itemId, cancellationToken);
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

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<ItemImageDto>>> GetItemImagesByItemId(Guid itemId, CancellationToken cancellationToken)
        {
            var result = await _getItemImagesByItemId.GetItemImagesByItemId(itemId, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<bool>> ToggleItemLike(ToggleItemLikeCommand command, CancellationToken cancellationToken)
        {
            var result = await _toggleItemLike.ToggleItemLike(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Guid>> UpdateItemImages(UpdateItemImagesCommand command, CancellationToken cancellationToken)
        {
            var result = await _updateItemImages.UpdateItemImages(command, cancellationToken);
            return this.ToActionResult(result);
        }
    }
}
