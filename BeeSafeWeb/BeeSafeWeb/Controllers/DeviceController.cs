using System.ComponentModel.DataAnnotations;
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

    public DeviceController(IRepository<Device> deviceRepository)
    {
        _deviceRepository = deviceRepository;
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
            IsOnline = true,
        };

        _deviceRepository.Add(device);

        RegisterResponse response = new ()
        {
            Id = device.Id,
        };

        return Ok(response);
    }

    private IActionResult MessageIsValid(RequestMessage message)
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

        return Ok();
    }

    [HttpPost("Ping")]
    public IActionResult Ping(RequestMessage requestMessage)
    {
        var result = MessageIsValid(requestMessage);

        if (result is not OkResult)
        {
            return result;
        }

        if (requestMessage.MessageType != MessageType.Ping)
        {
            return BadRequest();
        }

        var response = new ResponseMessage()
        {
            MessageType = MessageType.Pong
        };

        return Ok(response);
    }

    [HttpPost("DetectionEvent")]
    public IActionResult DetectionEvent(RequestMessage requestMessage)
    {
        var result = MessageIsValid(requestMessage);

        if (result is not OkResult)
        {
            return result;
        }

        if (requestMessage.MessageType != MessageType.DetectionEvent)
        {
            return BadRequest();
        }

        return Ok();
    }
}