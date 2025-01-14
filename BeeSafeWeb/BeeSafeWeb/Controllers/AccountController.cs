using Microsoft.AspNetCore.Mvc;

namespace BeeSafeWeb.Controllers;

public class AccountController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}