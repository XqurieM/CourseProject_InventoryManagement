using Ardalis.Result.AspNetCore;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.InventoryCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.InventoryQueries;
using Microsoft.AspNetCore.Http;
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

        public InventoryController(ICreateInventory createInventory, IGetInventoryById getInventoryById, IAddInventoryField addInventoryField)
        {
            _createInventory = createInventory;
            _getInventoryById = getInventoryById;
            _addInventoryField = addInventoryField;
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> CreateInventory(CreateInventoryCommand command,CancellationToken cancellationToken)
        {
            var result = await _createInventory.CreateInventory(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [HttpGet]
        public async Task<ActionResult<InventoryDto>> GetInventoryById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetInventoryByIdQuery { Id = id };

            var result = await _getInventoryById.GetInventoryById(query, cancellationToken);

            return this.ToActionResult(result);
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> AddInventoryField(AddInventoryFieldCommand command, CancellationToken cancellationToken)
        {
            var result = await _addInventoryField.AddInventoryField(command, cancellationToken);
            return this.ToActionResult(result);
        }       

    }
}
