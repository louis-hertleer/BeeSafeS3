using BeeSafeWeb.Data;
using Microsoft.AspNetCore.Mvc;

namespace BeeSafeWeb.Controllers;

public class VisitorController : Controller
{
    private readonly BeeSafeContext _context;

    public VisitorController(BeeSafeContext context)
    {
        _context = context;
    }
    

    // GET
    public IActionResult Index()
    {
        // Retrieve all nest estimates from the database
        var nestEstimates = _context.NestEstimates
            .Select(n => new
            {
                n.EstimatedLatitude,
                n.EstimatedLongitude,
                n.AccuracyLevel,
                n.Timestamp,
                n.IsDestroyed
            })
            .ToList();

        // Pass data to the view
        ViewData["NestEstimates"] = nestEstimates;

        return View();
    }
}