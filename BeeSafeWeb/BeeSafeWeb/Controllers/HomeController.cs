using System.Diagnostics;
using System.Text.Json;
using BeeSafeWeb.Data;
using BeeSafeWeb.Models;
using BeeSafeWeb.Services;
using BeeSafeWeb.Utility.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BeeSafeWeb.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IRepository<Device> _deviceRepository;
        private readonly IRepository<NestEstimate> _nestEstimateRepository;
        private readonly NestLocationService _nestLocationService;
        private readonly ILogger<HomeController> _logger;
    
        public HomeController(
            IRepository<Device> deviceRepository,
            IRepository<NestEstimate> nestEstimateRepository,
            NestLocationService nestLocationService,
            ILogger<HomeController> logger)
        {
            _deviceRepository = deviceRepository;
            _nestEstimateRepository = nestEstimateRepository;
            _nestLocationService = nestLocationService;
            _logger = logger;
        }
    
        /// <summary>
        /// Retrieves approved devices and nest estimates from the repository and passes them to the view.
        /// (Nest estimates are not recalculated on every request so that manual overrides are preserved.)
        /// </summary>
        public async Task<IActionResult> Index()
        {

            // Retrieve approved devices.
            var devices = await Task.Run(() =>
                _deviceRepository.GetQueryable()
                    .Where(d => d.IsApproved && !d.IsDeclined)
                    .Select(d => new
                    {
                        d.Id,
                        d.Latitude,
                        d.Longitude,
                        d.IsOnline,
                        d.IsTracking,
                        d.LastActiveString,
                        d.Name,
                        d.Direction
                    })
                    .ToList());

            // Retrieve nest estimates from repository.
            var nestEstimates = await _nestLocationService.CalculateAndPersistNestLocationsAsync();


         

            // (Optionally filter by radius—currently commented out.)
            var filteredNestEstimates = nestEstimates.ToList();
    
            // Prepare nest data for display.
            var nestData = filteredNestEstimates.Select(n => new
            {
                id = n.Id,
                lat = n.DisplayLatitude ?? n.EstimatedLatitude,
                lng = n.DisplayLongitude ?? n.EstimatedLongitude,
                radius = n.DisplayAccuracy ?? n.AccuracyLevel,
                LastUpdatedString = n.LastUpdatedString,
                IsDestroyed = n.IsDestroyed
            }).ToList();
    
            ViewData["Devices"] = devices;
            ViewData["NestEstimates"] = nestData;
    
            return View();
        }
    
        /// <summary>
        /// Updates the nest status (mark as destroyed or active) for a given nest estimate.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateNestStatus(Guid id, bool isDestroyed)
        {

            // Try finding the nest by ID first
            var nest = await _nestEstimateRepository.GetByIdAsync(id);

            if (nest == null)
            {

                // Retrieve all nests
                var allNests = await _nestEstimateRepository.GetQueryable().ToListAsync();

                if (!allNests.Any())
                {
                    return NotFound("Nest not found.");
                }

                // Use the first available nest as a reference
                var referenceNest = allNests.First(); 
                double referenceLat = referenceNest.EstimatedLatitude;
                double referenceLon = referenceNest.EstimatedLongitude;

                // Find the closest nest based on latitude and longitude
                var closestNest = allNests
                    .OrderBy(n => Math.Abs(n.EstimatedLatitude - referenceLat) + Math.Abs(n.EstimatedLongitude - referenceLon))
                    .FirstOrDefault();

                if (closestNest == null)
                {
                    return NotFound("Nest not found.");
                }

                nest = closestNest;
            }

            // Update destroy status
            nest.IsDestroyed = isDestroyed;
            await _nestEstimateRepository.UpdateAsync(nest);

            return RedirectToAction("Index");
        }



    
        public IActionResult Privacy() => View();
    
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
            });
    }
}
