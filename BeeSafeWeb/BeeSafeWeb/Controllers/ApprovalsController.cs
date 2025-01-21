using BeeSafeWeb.Data;
using BeeSafeWeb.Models;
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

    // GET
    public IActionResult Index()
    {
        var devices = _deviceRepository.GetQueryable()
            .Where(d => !d.IsApproved)
            .Where(d => !d.IsDeclined)
            .ToList();

        return View(devices);
    }

    [HttpPost("ApproveDevice/{id:guid}")]
    public IActionResult ApproveDevice(Guid id)
    {
        Device? device;

        device = _deviceRepository.GetById(id);
        if (device == null)
        {
            return NotFound();
        }

        device.IsApproved = true;
        device.LastActive = DateTime.Now;

        _deviceRepository.Update(device);

        return RedirectToAction("Index", "Approvals", new {});
    }

    [HttpPost("RejectDevice/{id:guid}")]
    public IActionResult RejectDevice(Guid id)
    {
        Device? device;

        device = _deviceRepository.GetById(id);
        if (device == null)
        {
            return NotFound();
        }

        /* XXX: probably not needed */
        device.IsApproved = false;
        device.IsDeclined = true;

        _deviceRepository.Update(device);

        return RedirectToAction("Index", "Approvals", new {});
    }
}
