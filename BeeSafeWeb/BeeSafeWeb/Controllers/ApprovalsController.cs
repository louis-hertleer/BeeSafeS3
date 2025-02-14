using System.Globalization;
using BeeSafeWeb.Data;
using BeeSafeWeb.Utility.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace BeeSafeWeb.Controllers
{
    [Authorize]
    public class ApprovalsController : Controller
    {
        private readonly IRepository<Device> _deviceRepository;

        public ApprovalsController(IRepository<Device> deviceRepository)
        {
            _deviceRepository = deviceRepository;
        }

        public async Task<IActionResult> Index()
        {
            var devices = _deviceRepository.GetQueryable()
                .Where(d => !d.IsApproved && !d.IsDeclined)
                .OrderBy(d => d.Id)
                .Select(d => new Device
                {
                    Id = d.Id,
                    Latitude = d.Latitude,
                    Longitude = d.Longitude,
                    Direction = d.Direction,
                    IsApproved = d.IsApproved,
                    IsDeclined = d.IsDeclined,
                    LastActive = d.LastActive
                })
                .ToList();

            return View(devices);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveDevice(Guid id, [FromForm] DeviceApprovalModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid input data.");
            }

            var device = await _deviceRepository.GetByIdAsync(id);
            if (device == null)
            {
                return NotFound();
            }

            // Update the device name if one is provided.
            if (!string.IsNullOrEmpty(model.Name))

            {
                device.Name = model.Name;
            }

            // read the raw form values and parse them using InvariantCulture.
            var latitudeStr = Request.Form["latitude"].ToString();
            var longitudeStr = Request.Form["longitude"].ToString();
            var directionStr = Request.Form["direction"].ToString();

            if (!double.TryParse(latitudeStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double latitude))
            {
                return BadRequest("Invalid latitude format.");
            }
            if (!double.TryParse(longitudeStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double longitude))
            {
                return BadRequest("Invalid longitude format.");
            }
            if (!double.TryParse(directionStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double direction))
            {
                return BadRequest("Invalid direction format.");
            }

            device.Latitude = latitude;
            device.Longitude = longitude;
            device.Direction = direction;
            device.IsApproved = true;
            device.LastActive = DateTime.UtcNow;

            await _deviceRepository.UpdateAsync(device);
            
            TempData["SuccessMessage"] = "Device updated successfully!";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RejectDevice(Guid id)
        {
            var device = await _deviceRepository.GetByIdAsync(id);
            if (device == null)
            {
                return NotFound();
            }

            device.IsApproved = false;
            device.IsDeclined = true;

            await _deviceRepository.UpdateAsync(device);
            return RedirectToAction(nameof(Index));
        }

        public class DeviceApprovalModel
        {
            public Guid Id { get; set; }
            public string? Name { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public double Direction { get; set; }
        }
    }
}
