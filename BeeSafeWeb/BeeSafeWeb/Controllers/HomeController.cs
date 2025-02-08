using System.Text.Json;
using BeeSafeWeb.Data;
using BeeSafeWeb.Messages;
using BeeSafeWeb.Models;
using BeeSafeWeb.Utility.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using BeeSafeWeb.Services;

namespace BeeSafeWeb.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IRepository<Device> _deviceRepository;
        private readonly IRepository<NestEstimate> _nestEstimateRepository;
        private readonly NestLocationService _nestLocationService;

        public HomeController(
            IRepository<Device> deviceRepository, 
            IRepository<NestEstimate> nestEstimateRepository,
            NestLocationService nestLocationService)
        {
            _deviceRepository = deviceRepository;
            _nestEstimateRepository = nestEstimateRepository;
            _nestLocationService = nestLocationService;
        }

        public async Task<IActionResult> Index()
        {
            // Retrieve devices that are approved and not declined.
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

            // Use the same nest location calculation as in the Visitors controller.
            // This method returns aggregated nest estimates.
            var nestEstimates = await _nestLocationService.CalculateNestLocationsAsync();

            // Project the nest data into an anonymous type.
            // Note: We use the aggregated display properties if available.
            var nestData = nestEstimates.Select(n => new
            {
                lat = n.DisplayLatitude ?? n.EstimatedLatitude,
                lng = n.DisplayLongitude ?? n.EstimatedLongitude,
                radius = n.DisplayAccuracy ?? n.AccuracyLevel,
                LastUpdatedString = n.LastUpdatedString, // Formatted string for last update
                IsDestroyed = n.IsDestroyed
            }).ToList();

            ViewData["Devices"] = devices;
            ViewData["NestEstimates"] = nestData;

            return View();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
