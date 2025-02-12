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
            _logger.LogDebug("Index action started.");

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
            _logger.LogDebug($"Fetched {devices.Count()} devices.");

            // Retrieve nest estimates from repository.
            var nestEstimates = await _nestLocationService.CalculateAndPersistNestLocationsAsync();


            // (For debugging, you can log each nest estimate.)
            foreach (var n in nestEstimates)
            {
                _logger.LogDebug($"NestEstimate: ID={n.Id}, Lat={n.EstimatedLatitude:F6}, Lon={n.EstimatedLongitude:F6}, Direction={n.Direction:F2}, IsDestroyed={n.IsDestroyed}");
            }

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
            _logger.LogDebug("Passing device and nest data to the view.");
    
            ViewData["Devices"] = devices;
            ViewData["NestEstimates"] = nestData;
            _logger.LogDebug("Index loaded with {DeviceCount} devices and {NestCount} nests", devices.Count, nestData.Count);
    
            return View();
        }
    
        /// <summary>
        /// Updates the nest status (mark as destroyed or active) for a given nest estimate.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateNestStatus(Guid id, bool isDestroyed)
        {
            _logger.LogDebug("UpdateNestStatus called with id: {Id}, isDestroyed: {IsDestroyed}", id, isDestroyed);
            if (id == Guid.Empty)
            {
                _logger.LogWarning("Empty Guid received in UpdateNestStatus");
                return NotFound("Invalid nest id");
            }
    
            var nest = await _nestEstimateRepository.GetByIdAsync(id);
            if (nest == null)
            {
                _logger.LogWarning("No nest found for id: {Id}", id);
                return NotFound($"Nest not found for id: {id}");
            }
    
            _logger.LogDebug("Found nest: {NestData}", JsonSerializer.Serialize(nest));
            nest.IsDestroyed = isDestroyed;
            await _nestEstimateRepository.UpdateAsync(nest);
            _logger.LogDebug("Nest updated. New IsDestroyed: {IsDestroyed}", isDestroyed);
    
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
