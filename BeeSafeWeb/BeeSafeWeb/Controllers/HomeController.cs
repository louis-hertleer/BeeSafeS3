using System.Text.Json;
using BeeSafeWeb.Data;
using BeeSafeWeb.Messages;
using BeeSafeWeb.Models;
using BeeSafeWeb.Utility.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

namespace BeeSafeWeb.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IRepository<Device> _deviceRepository;
    private readonly IRepository<NestEstimate> _nestEstimateRepository;

    public HomeController(IRepository<Device> deviceRepository, IRepository<NestEstimate> nestEstimateRepository)
    {
        _deviceRepository = deviceRepository;
        _nestEstimateRepository = nestEstimateRepository;
    }

    public async Task<IActionResult> Index()
    {
        var devices = await Task.Run(() => _deviceRepository.GetQueryable()
            .Where(d => d.IsApproved && !d.IsDeclined)
            .Select(d => new
            {
                d.Id,
                d.Latitude,
                d.Longitude,
                d.IsOnline,
                d.IsTracking,
                d.LastActiveString
            }).ToList());

        var nestEstimates = (await _nestEstimateRepository.GetAllAsync())
            .Select(n => new
            {
                n.EstimatedLatitude,
                n.EstimatedLongitude,
                n.AccuracyLevel,
                n.IsDestroyed,
                LastUpdatedString = n.LastUpdatedString // Add the formatted string
            }).ToList();

        ViewData["Devices"] = devices;
        ViewData["NestEstimates"] = nestEstimates;

        return View();
    }


    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}