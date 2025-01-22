using BeeSafeWeb.Data;
using BeeSafeWeb.Utility.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeeSafeWeb.Controllers;

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
        var devices = await Task.Run(() => _deviceRepository.GetQueryable()
            .Where(d => !d.IsApproved && !d.IsDeclined)
            .ToList());

        return View(devices);
    }

    [HttpPost("ApproveDevice/{id:guid}")]
    public async Task<IActionResult> ApproveDevice(Guid id)
    {
        var device = await _deviceRepository.GetByIdAsync(id);
        if (device == null)
        {
            return NotFound();
        }

        device.IsApproved = true;
        device.LastActive = DateTime.Now;

        await _deviceRepository.UpdateAsync(device);

        return RedirectToAction("Index", "Approvals");
    }

    [HttpPost("RejectDevice/{id:guid}")]
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

        return RedirectToAction("Index", "Approvals");
    }
}