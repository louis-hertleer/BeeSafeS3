using BeeSafeWeb.Data;
using BeeSafeWeb.Services;
using BeeSafeWeb.Utility.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BeeSafeWeb.Controllers
{
    public class VisitorController : Controller
    {
        private readonly NestLocationService _nestLocationService;

        public VisitorController(NestLocationService nestLocationService)
        {
            _nestLocationService = nestLocationService;
        }

        public async Task<IActionResult> Index()
        {
            var nestEstimates = await _nestLocationService.CalculateNestLocationsAsync();

            // Project the aggregated (display) properties and use "IsDestroyed" (uppercase) for consistency.
            var nestData = nestEstimates.Select(n => new
            {
                lat = n.DisplayLatitude ?? n.EstimatedLatitude,
                lng = n.DisplayLongitude ?? n.EstimatedLongitude,
                radius = n.DisplayAccuracy ?? n.AccuracyLevel,
                timestamp = n.Timestamp,
                IsDestroyed = n.IsDestroyed
            }).ToList();

            ViewData["NestEstimates"] = nestData;

            return View();
        }
    }
}