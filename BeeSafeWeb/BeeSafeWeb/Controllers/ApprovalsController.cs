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

            if (model.Name != null && model.Name.Length > 1)
            {
                device.Name = model.Name;
            }
            device.Latitude = model.Latitude;
            device.Longitude = model.Longitude;
            device.Direction = model.Direction;
            device.IsApproved = true;
            device.LastActive = DateTime.UtcNow;

            await _deviceRepository.UpdateAsync(device);
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
