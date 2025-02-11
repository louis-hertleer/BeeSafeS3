using System;
using System.Linq;
using System.Threading.Tasks;
using BeeSafeWeb.Data;
using BeeSafeWeb.Utility.Models;
using Microsoft.AspNetCore.Mvc;

namespace BeeSafeWeb.Controllers
{
    public class ManualDirectionController : Controller
    {
        private readonly IRepository<Device> _deviceRepository;
        private readonly IRepository<DetectionEvent> _detectionEventRepository;

        public ManualDirectionController(IRepository<Device> deviceRepository,
                                           IRepository<DetectionEvent> detectionEventRepository)
        {
            _deviceRepository = deviceRepository;
            _detectionEventRepository = detectionEventRepository;
        }

        // GET: /ManualDirection/
        public IActionResult Index()
        {
            // Retrieve all approved devices.
            var devices = _deviceRepository.GetQueryable()
                .Where(d => d.IsApproved)
                .ToList();
            return View(devices);
        }

        // POST: /ManualDirection/SubmitManualDetection
        [HttpPost]
        public async Task<IActionResult> SubmitManualDetection(ManualDetectionInputModel input)
        {
            if (!ModelState.IsValid)
            {
                var devices = _deviceRepository.GetQueryable().Where(d => d.IsApproved).ToList();
                return View("Index", devices);
            }

            if (!Guid.TryParse(input.DeviceId, out Guid id))
            {
                return BadRequest("Invalid device ID.");
            }

            var device = await _deviceRepository.GetByIdAsync(id);
            if (device == null)
            {
                return NotFound();
            }

            // Use defaults if not provided:
            int count = input.NumberOfHornets > 0 ? input.NumberOfHornets : 1;
            double flightTimeMinutes = input.FlightTimeMinutes > 0 ? input.FlightTimeMinutes : 0.5;
            TimeSpan flightTime = TimeSpan.FromMinutes(flightTimeMinutes);
            DateTime now = DateTime.Now;

            for (int i = 0; i < count; i++)
            {
                var detectionEvent = new DetectionEvent
                {
                    Id = Guid.NewGuid(),
                    FirstDetection = now,
                    SecondDetection = now.Add(flightTime),
                    HornetDirection = input.HornetDirection,
                    Device = device
                };

                await _detectionEventRepository.AddAsync(detectionEvent);
            }

            // Redirect to Home (or any page) so that the new detection events are used in nest calculations.
            return RedirectToAction("Index", "Home");
        }
    }

    public class ManualDetectionInputModel
    {
        public string DeviceId { get; set; }
        public double HornetDirection { get; set; }
        public int NumberOfHornets { get; set; }
        public double FlightTimeMinutes { get; set; }
    }
}
