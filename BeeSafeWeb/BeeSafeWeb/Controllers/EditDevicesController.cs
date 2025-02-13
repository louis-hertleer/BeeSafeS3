using System.Globalization;
using BeeSafeWeb.Data;
using BeeSafeWeb.Utility.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BeeSafeWeb.Controllers
{
    [Authorize]
    public class EditDevicesController : Controller
    {
        private readonly IRepository<Device> _deviceRepository;

        public EditDevicesController(IRepository<Device> deviceRepository)
        {
            _deviceRepository = deviceRepository;
        }

        public async Task<IActionResult> Index()
        {
            var devices = await _deviceRepository.GetQueryable()
                .Where(d => d.IsApproved && !d.IsDeclined)
                .Select(d => new
                {
                    d.Id,
                    d.Name,
                    d.Latitude,
                    d.Longitude,
                    d.Direction,
                    d.IsOnline,
                    d.IsTracking,
                    d.LastActiveString
                }).ToListAsync();

            return View(devices);
        }

        [HttpPost]
        public async Task<IActionResult> Update(Guid id, [FromForm] ApprovalsController.DeviceApprovalModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var device = await _deviceRepository.GetByIdAsync(id);
            if (device == null)
            {
                return NotFound();
            }

            // Update the device name if it has changed and a new name was provided.
            if (device.Name != model.Name && model.Name != null)
            {
                device.Name = model.Name;
            }

            // Read raw form values and force the correct decimal separator.
            var latitudeStr = Request.Form["latitude"].ToString().Replace(',', '.');
            var longitudeStr = Request.Form["longitude"].ToString().Replace(',', '.');
            var directionStr = Request.Form["direction"].ToString().Replace(',', '.');

            double latitude, longitude, direction;

            if (string.IsNullOrWhiteSpace(latitudeStr))
            {
                latitude = device.Latitude;
            }
            else if (!double.TryParse(latitudeStr, NumberStyles.Float, CultureInfo.InvariantCulture, out latitude))
            {
                return BadRequest("Invalid latitude format.");
            }

            if (string.IsNullOrWhiteSpace(longitudeStr))
            {
                longitude = device.Longitude;
            }
            else if (!double.TryParse(longitudeStr, NumberStyles.Float, CultureInfo.InvariantCulture, out longitude))
            {
                return BadRequest("Invalid longitude format.");
            }

            if (string.IsNullOrWhiteSpace(directionStr))
            {
                direction = device.Direction;
            }
            else if (!double.TryParse(directionStr, NumberStyles.Float, CultureInfo.InvariantCulture, out direction))
            {
                return BadRequest("Invalid direction format.");
            }

            device.Latitude = latitude;
            device.Longitude = longitude;
            device.Direction = direction;

            await _deviceRepository.UpdateAsync(device);

            // Set a TempData flag/message for success
            TempData["SuccessMessage"] = "Device updated successfully!";

            return RedirectToAction(nameof(Index));
        }
    }
}
