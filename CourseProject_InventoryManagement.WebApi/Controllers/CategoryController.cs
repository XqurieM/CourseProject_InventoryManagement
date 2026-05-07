using Ardalis.Result.AspNetCore;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.CategoryCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.InventoryCommands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static CourseProject_InventoryManagement.Application.Features.CQRS.ICQRS;

namespace CourseProject_InventoryManagement.WebApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICreateCategory _createCategory;
        private readonly IUpdateCategory _updateCategory;
        private readonly IDeleteCategory _deleteCategory;
        private readonly IGetAllCategories _getAllCategories;
        private readonly IGetCategoryById _getCategoryById;

        public CategoryController(ICreateCategory createCategory, IUpdateCategory updateCategory, IDeleteCategory deleteCategory, IGetAllCategories getAllCategories, IGetCategoryById getCategoryById)
        {
            _createCategory = createCategory;
            _updateCategory = updateCategory;
            _deleteCategory = deleteCategory;
            _getAllCategories = getAllCategories;
            _getCategoryById = getCategoryById;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateCategory(CreateCategoryCommand command, CancellationToken cancellationToken)
        {
            var result = await _createCategory.CreateCategory(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Guid>> UpdateCategory(UpdateCategoryCommand command, CancellationToken cancellationToken)
        {
            var result = await _updateCategory.UpdateCategory(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Guid>> DeleteCategory(DeleteCategoryCommand command, CancellationToken cancellationToken)
        {
            var result = await _deleteCategory.DeleteCategory(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<List<CategoryDto>>> GetAllCategories(CancellationToken cancellationToken)
        {
            var result = await _getAllCategories.GetAllCategories(cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<CategoryDto>> GetCategoryById(Guid categoryId,CancellationToken cancellationToken)
        {
            var result = await _getCategoryById.GetCategoryById(categoryId,cancellationToken);
            return this.ToActionResult(result);
        }
    }
}
