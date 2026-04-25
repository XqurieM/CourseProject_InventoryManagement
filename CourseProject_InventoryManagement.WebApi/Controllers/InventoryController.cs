using Ardalis.Result.AspNetCore;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.InventoryCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.InventoryQueries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static CourseProject_InventoryManagement.Application.Features.CQRS.ICQRS;

namespace CourseProject_InventoryManagement.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly ICreateInventory _createInventory;
        private readonly IGetInventoryById _getInventoryById;

        public InventoryController(ICreateInventory createInventory, IGetInventoryById getInventoryById)
        {
            _createInventory = createInventory;
            _getInventoryById = getInventoryById;
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create(CreateInventoryCommand command,CancellationToken cancellationToken)
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

    }
}
