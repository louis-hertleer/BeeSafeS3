namespace BeeSafeWeb.Models;

public class DetectionEvent
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    /// <summary>
    /// The offset, in degrees, where the hornet was travelling.
    /// </summary>
    public double HornetDirection { get; set; }
    public DateTime FirstDetection { get; set; }
    public DateTime SecondDetection { get; set; }
    
    /// <summary>
    /// The device to which this detection event belongs.
    /// </summary>
    public Device? Device { get; set; }
    
    public KnownHornet? KnownHornet { get; set; }
}