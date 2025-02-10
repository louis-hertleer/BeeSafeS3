using BeeSafeWeb.Data;
using BeeSafeWeb.Utility.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeeSafeWeb.Controllers;

[Authorize]
public class DetectionController : Controller
{
    private readonly IRepository<DetectionEvent> _detectionEventRepository;
    private readonly IRepository<Device> _devicesRepository;
    
    public DetectionController(IRepository<DetectionEvent> detectionEventRepository, 
                               IRepository<Device> devicesRepository)
    {
        _detectionEventRepository = detectionEventRepository;
        _devicesRepository = devicesRepository;
    }
    
    // GET
    public async Task<IActionResult> Index(Guid? device)
    {
        /* this is a MASSIVE hack */
        IQueryable<DetectionEvent> qDetections;

        qDetections = _detectionEventRepository.GetQueryable();

        if (device != null)
        {
            qDetections = qDetections.Where(de => de.Device != null && de.Device.Id == device);
        }

        IEnumerable<dynamic> detections = qDetections.Select(de => new
                                                      {
                                                          de.Id,
                                                          de.Device,
                                                          de.Timestamp,
                                                          de.HornetDirection
                                                      })
                                                      .ToList();

        var devices = _devicesRepository.GetQueryable()
            .Select(d => new
            {
                d.Id,
                d.Latitude,
                d.Longitude,
                d.Direction,
                d.IsOnline,
                d.IsTracking,
                d.IsApproved,
                d.LastActiveString,
                d.IsDeclined
            })
            .Where(d => d.IsApproved && !d.IsDeclined)
            .ToList();
        ViewData["Devices"] = devices;
        return View(detections);
    }
}