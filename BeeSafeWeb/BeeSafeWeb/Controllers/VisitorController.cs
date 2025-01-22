using BeeSafeWeb.Data;
using BeeSafeWeb.Utility.Models;
using Microsoft.AspNetCore.Mvc;

namespace BeeSafeWeb.Controllers;

public class VisitorController : Controller
{
    private readonly IRepository<NestEstimate> _nestRepository;

    public VisitorController(IRepository<NestEstimate> nestRepository)
    {
        _nestRepository = nestRepository;
    }

    public async Task<IActionResult> Index()
    {
        var nestEstimates = (await _nestRepository.GetAllAsync())
            .Select(n => new
            {
                n.EstimatedLatitude,
                n.EstimatedLongitude,
                n.AccuracyLevel,
                n.Timestamp,
                n.IsDestroyed
            }).ToList();

        ViewData["NestEstimates"] = nestEstimates;

        return View();
    }

}