using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ClubActivityManager.Models;

namespace ClubActivityManager.Controllers;
//ciocan
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return RedirectToAction("Landing");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Landing()
    {
        return Redirect("/pages/landing.html");
    }
}
