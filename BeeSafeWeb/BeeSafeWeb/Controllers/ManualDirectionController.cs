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

            _logger.LogDebug("ManualDirectionController.Index: {DeviceCount} devices, {DetectionCount} manual detections", devices.Count, manualDetections.Count);

            var model = new ManualDirectionIndexViewModel
            {
                Devices = devices,
                ManualDetections = manualDetections
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
            _logger.LogDebug("SubmitManualDetection called: DeviceId={DeviceId}, HornetDirection={HornetDirection}, NumberOfHornets={NumberOfHornets}, FlightTimeMinutes={FlightTimeMinutes}",
                input.DeviceId, input.HornetDirection, input.NumberOfHornets, input.FlightTimeMinutes);

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
                        .ToList()
                };
                return View("Index", model);
            }

            if (!Guid.TryParse(input.DeviceId, out Guid deviceId))
            {
                _logger.LogWarning("SubmitManualDetection: Invalid device ID: {DeviceId}", input.DeviceId);
                return BadRequest("Invalid device ID.");
            }

            var device = await _deviceRepository.GetByIdAsync(deviceId);
            if (device == null)
            {
                _logger.LogWarning("SubmitManualDetection: Device not found: {DeviceId}", deviceId);
                return NotFound();
            }

            int numberOfHornets = input.NumberOfHornets > 0 ? input.NumberOfHornets : 1;
            double flightTimeMinutes = input.FlightTimeMinutes > 0 ? input.FlightTimeMinutes : 0.5;
            TimeSpan flightTime = TimeSpan.FromMinutes(flightTimeMinutes);
            DateTime now = DateTime.Now;

            _logger.LogDebug("SubmitManualDetection: Now={Now}, FlightTimeMinutes={FlightTimeMinutes}", now, flightTimeMinutes);

            TimeSpan groupingWindow = TimeSpan.FromMinutes(5);
            var existingManualEvent = _detectionEventRepository.GetQueryable()
                .Include(e => e.Device)
                .Where(e => e.Device != null && e.Device.Id == device.Id && e.IsManual)
                .OrderByDescending(e => e.SecondDetection)
                .FirstOrDefault();

            if (existingManualEvent != null && (now - existingManualEvent.SecondDetection) < groupingWindow)
            {
                double directionDifference = System.Math.Abs(existingManualEvent.HornetDirection - input.HornetDirection);
                if (directionDifference > 180)
                    directionDifference = 360 - directionDifference;
                const double directionThreshold = 45;
                _logger.LogDebug("Existing manual event found: {EventId}, DirectionDifference={Difference}", existingManualEvent.Id, directionDifference);

                if (directionDifference <= directionThreshold)
                {
                    existingManualEvent.HornetCount += numberOfHornets;
                    existingManualEvent.HornetDirection = (existingManualEvent.HornetDirection + input.HornetDirection) / 2;
                    existingManualEvent.SecondDetection = now;
                    _logger.LogDebug("Grouping with existing manual event: new HornetCount={Count}, new HornetDirection={Dir}", existingManualEvent.HornetCount, existingManualEvent.HornetDirection);
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
                    _logger.LogDebug("Creating separate manual event due to direction difference. New event ID: {NewId}", newEvent.Id);
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
                _logger.LogDebug("No recent manual event found; creating new event. New event ID: {NewId}", newEvent.Id);
                await _detectionEventRepository.AddAsync(newEvent);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Displays the edit form for a manual detection event.
        /// </summary>
        public async Task<IActionResult> Edit(Guid id)
        {
            var detection = await _detectionEventRepository.GetByIdAsync(id);
            if (detection == null || !detection.IsManual)
            {
                _logger.LogWarning("Edit: Detection event not found or not manual. ID: {Id}", id);
                return NotFound();
            }

            var model = new ManualDetectionInputModel
            {
                DeviceId = detection.Device?.Id.ToString(),
                HornetDirection = detection.HornetDirection,
                NumberOfHornets = detection.HornetCount,
                FlightTimeMinutes = (detection.SecondDetection - detection.FirstDetection).TotalMinutes
            };

            _logger.LogDebug("Edit GET: Returning model for detection event ID {Id}", id);
            return View(model);
        }

        /// <summary>
        /// Updates a manual detection event.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ManualDetectionInputModel input)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Edit POST: Model state invalid for detection event ID {Id}", id);
                return View(input);
            }

            var detection = await _detectionEventRepository.GetByIdAsync(id);
            if (detection == null || !detection.IsManual)
            {
                _logger.LogWarning("Edit POST: Detection event not found or not manual. ID: {Id}", id);
                return NotFound();
            }

            detection.HornetDirection = input.HornetDirection;
            detection.HornetCount = input.NumberOfHornets;
            detection.SecondDetection = DateTime.Now.AddMinutes(input.FlightTimeMinutes);

            _logger.LogDebug("Edit POST: Updating detection event ID {Id}", id);
            await _detectionEventRepository.UpdateAsync(detection);
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
                _logger.LogWarning("Delete: Detection event not found or not manual. ID: {Id}", id);
                return NotFound();
            }

            _logger.LogDebug("Deleting detection event ID {Id}", id);
            // Here we assume DeleteAsync accepts the entire detection event. Adjust if needed.
            await _detectionEventRepository.DeleteAsync(detection.Id);
            return RedirectToAction("Index");
        }
    }
}
