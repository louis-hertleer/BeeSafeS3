using System.Text.Json;
using BeeSafeWeb.Data;
using BeeSafeWeb.Messages;
using BeeSafeWeb.Utility.Models;
using Microsoft.AspNetCore.Mvc;

namespace BeeSafeWeb.Controllers;

[ApiController]
[Route("[controller]")]
public class DeviceController : Controller
{
    private readonly IRepository<Device> _deviceRepository;
    private readonly IRepository<DetectionEvent> _detectionRepository;

    public DeviceController(IRepository<Device> deviceRepository, IRepository<DetectionEvent> detectionRepository)
    {
        _deviceRepository = deviceRepository;
        _detectionRepository = detectionRepository;
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        Device device = new()
        {
            Longitude = request.Longitude,
            Latitude = request.Latitude,
            Direction = request.Direction,
            IsApproved = false,
        };

        await _deviceRepository.AddAsync(device);

        return Ok(new { Id = device.Id });
    }

    private async Task<ActionResult> MessageIsValid(RequestMessage message)
    {
        if (string.IsNullOrEmpty(message.Device) || !Guid.TryParse(message.Device, out var guid))
        {
            return Unauthorized();
        }

        var device = await Task.Run(() => _deviceRepository.GetByIdAsync(guid));
        if (device == null)
        {
            return Unauthorized();
        }

        if (!device.IsApproved)
        {
            return StatusCode(403);
        }

        return Ok(device);
    }

    [HttpPost("Ping")]
    public async Task<IActionResult> Ping(RequestMessage requestMessage)
    {
        var result = await MessageIsValid(requestMessage);

        if (result is not OkObjectResult okResult)
        {
            return result;
        }

        var device = (Device)okResult.Value;
        device.LastActive = DateTime.Now;
        await _deviceRepository.UpdateAsync(device);

        return Ok(new ResponseMessage { MessageType = MessageType.Pong });
    }

    [HttpPost("DetectionEvent")]
    public async Task<IActionResult> DetectionEvent(RequestMessage requestMessage)
    {
        var result = await MessageIsValid(requestMessage);

        if (result is not OkObjectResult okResult)
        {
            return result;
        }

        var device = (Device)okResult.Value;

        if (requestMessage.MessageType != MessageType.DetectionEvent || requestMessage.Data is null)
        {
            return BadRequest();
        }

        try
        {
            var data = requestMessage.Data;
            int timestamp = data.GetProperty("timestamp").GetInt32();
            float hornetDirection = data.GetProperty("hornet_direction").GetSingle();

            DetectionEvent detectionEvent = new()
            {
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime,
                HornetDirection = hornetDirection,
                Device = device
            };

            await  _detectionRepository.AddAsync(detectionEvent);
        }
        catch (Exception)
        {
            return BadRequest("Invalid data format.");
        }

        return Ok();
    }
}
