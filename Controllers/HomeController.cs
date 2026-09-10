using Microsoft.AspNetCore.Mvc;

namespace FabricDevelopmentHub.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
