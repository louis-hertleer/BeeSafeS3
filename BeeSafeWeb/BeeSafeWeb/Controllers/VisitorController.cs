using Microsoft.AspNetCore.Mvc;

namespace BeeSafeWeb.Controllers;

public class VisitorController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}