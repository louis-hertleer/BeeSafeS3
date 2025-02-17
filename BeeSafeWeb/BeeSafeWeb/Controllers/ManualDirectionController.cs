using System;
using System.Linq;
using System.Threading.Tasks;
using BeeSafeWeb.Data;
using BeeSafeWeb.Models; // Ensure view models are in BeeSafeWeb.Models
using BeeSafeWeb.Utility.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BeeSafeWeb.Controllers
{
    [Authorize]
    public class ManualDirectionController : Controller
    {
        private readonly IRepository<Device> _deviceRepository;
        private readonly IRepository<DetectionEvent> _detectionEventRepository;
        private readonly ILogger<ManualDirectionController> _logger;

        public ManualDirectionController(
            IRepository<Device> deviceRepository,
            IRepository<DetectionEvent> detectionEventRepository,
            ILogger<ManualDirectionController> logger)
        {
            _deviceRepository = deviceRepository;
            _detectionEventRepository = detectionEventRepository;
            _logger = logger;
        }

        /// <summary>
        /// Displays the Manual Direction Index page containing approved devices and manual detection events.
        /// </summary>
        public IActionResult Index()
        {
            var devices = _deviceRepository.GetQueryable()
                .Where(d => d.IsApproved)
                .ToList();

            var manualDetections = _detectionEventRepository.GetQueryable()
                .Include(e => e.Device)
                .Where(e => e.IsManual)
                .ToList();

            var model = new ManualDirectionIndexViewModel
            {
                Devices = devices,
                ManualDetections = manualDetections,
                // Initialize the ManualDetectionInput property
                ManualDetectionInput = new ManualDetectionInputModel()
            };

            return View(model);
        }

        /// <summary>
        /// Submits a manual detection event. Groups with an existing event if possible.
        /// </summary>
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> SubmitManualDetection(ManualDetectionInputModel input)
{
    if (!ModelState.IsValid)
    {
        var devices = _deviceRepository.GetQueryable()
            .Where(d => d.IsApproved)
            .ToList();
        var model = new ManualDirectionIndexViewModel
        {
            Devices = devices,
            ManualDetections = _detectionEventRepository.GetQueryable()
                .Include(e => e.Device)
                .Where(e => e.IsManual)
                .ToList(),
            // Pass the input model back to the view
            ManualDetectionInput = input
        };
        return View("Index", model);
    }

    if (!Guid.TryParse(input.DeviceId, out Guid deviceId))
    {
        return BadRequest("Invalid device ID.");
    }

    var device = await _deviceRepository.GetByIdAsync(deviceId);
    if (device == null)
    {
        return NotFound();
    }

    int numberOfHornets = input.NumberOfHornets > 0 ? input.NumberOfHornets : 1;
    double flightTimeMinutes = input.FlightTimeMinutes > 0 ? input.FlightTimeMinutes : 0.5;
    TimeSpan flightTime = TimeSpan.FromMinutes(flightTimeMinutes);
    DateTime now = DateTime.Now;

    TimeSpan groupingWindow = TimeSpan.FromMinutes(5);
    var existingManualEvent = _detectionEventRepository.GetQueryable()
        .Include(e => e.Device)
        .Where(e => e.Device != null && e.Device.Id == device.Id && e.IsManual)
        .OrderByDescending(e => e.SecondDetection)
        .FirstOrDefault();

    if (existingManualEvent != null && (now - existingManualEvent.SecondDetection) < groupingWindow)
    {
        double directionDifference =
            System.Math.Abs(existingManualEvent.HornetDirection - input.HornetDirection);
        if (directionDifference > 180)
            directionDifference = 360 - directionDifference;
        const double directionThreshold = 45;

        if (directionDifference <= directionThreshold)
        {
            existingManualEvent.HornetCount += numberOfHornets;
            existingManualEvent.HornetDirection =
                (existingManualEvent.HornetDirection + input.HornetDirection) / 2;
            existingManualEvent.SecondDetection = now;

            await _detectionEventRepository.UpdateAsync(existingManualEvent);
        }
        else
        {
            var newEvent = new DetectionEvent
            {
                Id = Guid.NewGuid(),
                FirstDetection = now,
                SecondDetection = now.Add(flightTime),
                HornetDirection = input.HornetDirection,
                IsManual = true,
                HornetCount = numberOfHornets,
                Device = device
            };

            await _detectionEventRepository.AddAsync(newEvent);
        }
    }
    else
    {
        var newEvent = new DetectionEvent
        {
            Id = Guid.NewGuid(),
            FirstDetection = now,
            SecondDetection = now.Add(flightTime),
            HornetDirection = input.HornetDirection,
            IsManual = true,
            HornetCount = numberOfHornets,
            Device = device
        };

        await _detectionEventRepository.AddAsync(newEvent);
    }

    return RedirectToAction("Index");
}
        
        /// <summary>
        /// Deletes a manual detection event.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var detection = await _detectionEventRepository.GetByIdAsync(id);
            if (detection == null || !detection.IsManual)
            {
                return NotFound();
            }

            await _detectionEventRepository.DeleteAsync(detection.Id);
            return RedirectToAction("Index");
        }
    }
}
