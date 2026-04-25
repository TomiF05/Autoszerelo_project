using Microsoft.AspNetCore.Mvc;

namespace Autoszerelo.Controllers
{
    public class WorksController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
