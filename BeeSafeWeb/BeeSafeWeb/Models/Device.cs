namespace BeeSafeWeb.Models;

/// <summary>
/// A device that detects hornets.
/// </summary>
public class Device
{
    public Guid Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    
    /// <summary>
    /// If true, the device is operating as usual. If false, that means the
    /// device has been marked offline after a period of not sending PING
    /// messages.
    /// </summary>
    public bool IsOnline { get; set; }
    
    /// <summary>
    /// If true, this means the device is tracking the hornets. If false, the
    /// device is attacking the hornets. 
    /// </summary>
    public bool IsTracking { get; set; }
    
    /// <summary>
    /// The number of degrees, relative from north, that the device's camera is
    /// pointed towards.
    /// </summary>
    public double Direction { get; set; }
    
    public List<DetectionEvent> DetectionEvents { get; set; }
}