using Microsoft.AspNetCore.Mvc;

namespace Lesson_8.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
