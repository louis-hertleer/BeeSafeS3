using System.Text.Json;
using BeeSafeWeb.Data;
using BeeSafeWeb.Messages;
using BeeSafeWeb.Models;
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
    public IActionResult Register(RegisterRequest request)
    {
        Device device = new()
        {
            Longitude = request.Longitude,
            Latitude = request.Latitude,
            Direction = request.Direction,
            IsApproved = false,
        };

        _deviceRepository.Add(device);

        RegisterResponse response = new ()
        {
            Id = device.Id,
        };

        return Ok(response);
    }

    private ActionResult MessageIsValid(RequestMessage message)
    {
        Device? device;
        Guid guid;

        if (String.IsNullOrEmpty(message.Device) || !Guid.TryParse(message.Device, out guid))
        {
            return Unauthorized();
        }

        device = _deviceRepository.GetById(guid);

        /* device must be in the system */
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
    public IActionResult Ping(RequestMessage requestMessage)
    {
        Device device;
        var result = MessageIsValid(requestMessage);

        if (result is not OkObjectResult)
        {
            return result;
        }

        if (requestMessage.MessageType != MessageType.Ping)
        {
            return BadRequest();
        }

        /* update the last active state */
        device = (result as OkObjectResult).Value as Device;
        device.LastActive = DateTime.Now;
        _deviceRepository.Update(device);

        var response = new ResponseMessage()
        {
            MessageType = MessageType.Pong
        };

        return Ok(response);
    }

    [HttpPost("DetectionEvent")]
    public IActionResult DetectionEvent(RequestMessage requestMessage)
    {
        Device device;
        float hornetDirection;
        Guid guid;
        int timestamp;
        var result = MessageIsValid(requestMessage);

        if (result is not OkObjectResult)
        {
            return result;
        }

        /* At this point, we can parse the guid successfully. If this fails,
         * IDK anymore
         */
        Guid.TryParse(requestMessage.Device, out guid);
        device = _deviceRepository.GetById(guid)!;

        if (requestMessage.MessageType != MessageType.DetectionEvent
            || requestMessage.Data is null)
        {
            return BadRequest();
        }

        JsonElement data = requestMessage.Data;

        try
        {
            timestamp = data.GetProperty("timestamp").GetInt32();
            hornetDirection = data.GetProperty("hornet_direction").GetSingle();
        }
        catch (KeyNotFoundException)
        {
            return BadRequest(
                "\"data\" attribute is missing either timestamp or hornet_direction.");
        }
        catch (InvalidOperationException)
        {
            return BadRequest();
        }

        DetectionEvent detectionEvent = new()
        {
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime,
            HornetDirection = hornetDirection,
            Device = device
        };

        _detectionRepository.Add(detectionEvent);

        return Ok();
    }
}