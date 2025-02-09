using BeeSafeWeb.Data;
using BeeSafeWeb.Services;
using BeeSafeWeb.Utility.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
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
            // Retrieve all persisted nest estimates
            var nestEstimates = await _nestLocationService.CalculateAndPersistNestLocationsAsync();

            // Prepare separate lists for active and destroyed nests
            var activeNests = nestEstimates.Where(n => n.IsDestroyed == false).ToList();
            var destroyedNests = nestEstimates.Where(n => n.IsDestroyed == true).ToList();

            // Create a view model or a tuple/dictionary to pass both lists to the view.
            ViewData["ActiveNests"] = activeNests.Select(n => new {
                lat = n.DisplayLatitude ?? n.EstimatedLatitude,
                lng = n.DisplayLongitude ?? n.EstimatedLongitude,
                radius = n.DisplayAccuracy ?? n.AccuracyLevel,
                timestamp = n.Timestamp,
                IsDestroyed = n.IsDestroyed
            }).ToList();

            ViewData["DestroyedNests"] = destroyedNests.Select(n => new {
                IsDestroyed = n.IsDestroyed
            }).ToList();

            return View();
        }

    }
}