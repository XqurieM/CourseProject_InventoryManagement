using Ardalis.Result.AspNetCore;
using CourseProject_InventoryManagement.Application.Features.CQRS;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.ItemCommands;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static CourseProject_InventoryManagement.Application.Features.CQRS.ICQRS;

namespace CourseProject_InventoryManagement.WebApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly ICQRS.IAddItem _addItem;
        private readonly ICQRS.IAddItemFieldValues _addItemFieldValues;

        public ItemController(IAddItem addItem, IAddItemFieldValues addItemFieldValues)
        {
            _addItem = addItem;
            _addItemFieldValues = addItemFieldValues;
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> AddItem(AddItemCommand command, CancellationToken cancellationToken)
        {
            var result = await _addItem.AddItem(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> AddItemFieldValues(AddItemFieldValuesCommand command, CancellationToken cancellationToken)
        {
            var result = await _addItemFieldValues.AddItemFieldValues(command, cancellationToken);
            return this.ToActionResult(result);
        }
    }
}
