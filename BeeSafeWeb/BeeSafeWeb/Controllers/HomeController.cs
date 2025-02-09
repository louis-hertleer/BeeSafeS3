using System.Diagnostics;
using System.Text.Json;
using BeeSafeWeb.Data;
using BeeSafeWeb.Models;
using BeeSafeWeb.Services;
using BeeSafeWeb.Utility.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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

        public async Task<IActionResult> Index()
        {
            // Retrieve devices that are approved and not declined.
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
                        d.LastActiveString
                    })
                    .ToList());

            // Calculate and persist nest estimates.
            var nestEstimates = await _nestLocationService.CalculateAndPersistNestLocationsAsync();

            // Project the nest data (using display properties when available).
            var nestData = nestEstimates.Select(n => new
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

            _logger.LogDebug("Index loaded with {DeviceCount} devices and {NestCount} nests", devices.Count, nestData.Count);

            return View();
        }

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
