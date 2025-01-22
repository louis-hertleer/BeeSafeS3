using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeeSafeWeb.Utility.Models;

/// <summary>
/// A device that detects hornets.
/// </summary>
public class Device
{
    public Guid Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    
    /// <summary>
    /// Whether the device has been approved yet.
    ///
    /// A device may send a registration message to the server, but it is up to
    /// the user of the application to approve it. Unapproved devices must not
    /// show up in the overview map.
    /// </summary>
    public bool IsApproved { get; set; }
    
    /// <summary>
    /// Whether the device has been declined.
    /// </summary>
    public bool IsDeclined { get; set; }

    /// <summary>
    /// If true, the device is operating as usual. If false, that means the
    /// device has been marked offline after a period of not sending PING
    /// messages.
    /// </summary>
    [NotMapped]
    public bool IsOnline => (DateTime.Now - LastActive).Minutes < 5;

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
    
    /// <summary>
    /// The time that the device was last heard from.
    /// </summary>
    public DateTime LastActive { get; set; }

    /// <summary>
    /// The text, in human-readable form, that describes how long ago the device
    /// was last updated.
    /// </summary>
    [NotMapped]
    public string LastActiveString
    {
        get
        {
            var date = DateTime.Now - LastActive;
            int number = 0;
            string unit = "Just now";
            if (date.Days > 0)
            {
                number = date.Days;
                unit = "day";
            }
            else if (date.Hours > 0)
            {
                number = date.Hours;
                unit = "hour";
            }
            else if (date.Minutes > 0)
            {
                number = date.Minutes;
                unit = "minute";
            }
            else if (date.Seconds > 30)
            {
                number = date.Seconds;
                unit = "second";
            }
            else
            {
                return unit;
            }

            if (number != 1)
            {
                unit += "s";
            }

            return $"{number} {unit} ago";
        }
    }

    public List<DetectionEvent> DetectionEvents { get; set; }
}