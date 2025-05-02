using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ClubActivityManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Serves Views/Home/Index.cshtml
        public IActionResult Index()
        {
            return View(); // No redirect
        }

        public IActionResult Login()
        {
            return View();
        }

    }
}
