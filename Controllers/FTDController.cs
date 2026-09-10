using Microsoft.AspNetCore.Mvc;

namespace FabricDevelopmentHub.Controllers
{
    public class FTDController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
    }
}
