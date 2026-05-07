using Ardalis.Result.AspNetCore;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.CategoryCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.InventoryCommands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static CourseProject_InventoryManagement.Application.Features.CQRS.ICQRS;

namespace CourseProject_InventoryManagement.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICreateCategory _createCategory;

        public CategoryController(ICreateCategory createCategory)
        {
            _createCategory = createCategory;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateCategory(CreateCategoryCommand command, CancellationToken cancellationToken)
        {
            var result = await _createCategory.CreateCategory(command, cancellationToken);
            return this.ToActionResult(result);
        }
    }
}
