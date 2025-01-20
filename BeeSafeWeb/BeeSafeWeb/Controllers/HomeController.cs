using Microsoft.AspNetCore.Mvc;
using BeeSafeWeb.Data;
using Microsoft.EntityFrameworkCore;
using BeeSafeWeb.Models;
using Microsoft.AspNetCore.Authorization;

using System.Diagnostics;

namespace BeeSafeWeb.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly BeeSafeContext _context;

        public HomeController(BeeSafeContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // get information from the devices
            var devices = _context.Devices
                .Select(d => new
                {
                    d.Id,
                    d.Latitude,
                    d.Longitude,
                    d.IsOnline,
                    d.IsTracking,
                    d.IsApproved,
                    d.LastActiveString,
                    d.IsDeclined,
                })
                .Where(d => d.IsApproved)
                .Where(d => !d.IsDeclined)
                .ToList();

            // get information from the nest estimate
            var nestEstimates = _context.NestEstimates
                .Select(n => new
                {
                    n.EstimatedLatitude,
                    n.EstimatedLongitude,
                    n.AccuracyLevel,
                    n.IsDestroyed,  // Add IsDestroyed to track active vs neutralized nests
                    n.Timestamp
                })
                .ToList();

            // Pass data to the view
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
}
