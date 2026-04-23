using Microsoft.AspNetCore.Mvc;

namespace CourseProject_InventoryManagement.WebApi.Controllers
{
    public class GeneralController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
