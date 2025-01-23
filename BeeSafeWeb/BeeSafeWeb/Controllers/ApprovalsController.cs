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

   [HttpPost("ApproveDevice")]
   public async Task<IActionResult> ApproveDevice(Guid id, string latitude, string longitude, string direction)
   {
       if (!double.TryParse(latitude, System.Globalization.CultureInfo.InvariantCulture, out double lat) ||
           !double.TryParse(longitude, System.Globalization.CultureInfo.InvariantCulture, out double lon) ||
           !double.TryParse(direction, System.Globalization.CultureInfo.InvariantCulture, out double dir))
       {
           return BadRequest("Invalid latitude, longitude, or direction format.");
       }
   
       var device = await _deviceRepository.GetByIdAsync(id);
       if (device == null)
       {
           return NotFound();
       }
   
       device.Latitude = lat;
       device.Longitude = lon;
       device.Direction = dir;
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