using System.Collections.Generic;
using BeeSafeWeb.Utility.Models;

namespace BeeSafeWeb.Models
{
    /// <summary>
    /// View model for the Manual Direction Index page.
    /// Contains both approved devices and manually added detection events.
    /// </summary>
    public class ManualDirectionIndexViewModel
    {
        public IEnumerable<Device> Devices { get; set; }
        public IEnumerable<DetectionEvent> ManualDetections { get; set; }
        
        public ManualDetectionInputModel ManualDetectionInput { get; set; }

    }
}