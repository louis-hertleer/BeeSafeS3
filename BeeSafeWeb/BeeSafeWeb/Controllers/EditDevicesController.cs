using BeeSafeWeb.Data;
using BeeSafeWeb.Utility.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeeSafeWeb.Controllers;

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

        if (device.Name != model.Name && model.Name != null)
        {
            device.Name = model.Name;
        }
        device.Latitude = model.Latitude;
        device.Longitude = model.Longitude;
        device.Direction = model.Direction;

        await _deviceRepository.UpdateAsync(device);
        return RedirectToAction(nameof(Index));
    }
}