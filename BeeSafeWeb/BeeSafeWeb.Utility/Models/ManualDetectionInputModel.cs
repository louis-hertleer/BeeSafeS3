using System;
using System.ComponentModel.DataAnnotations;

namespace BeeSafeWeb.Models
{
    /// <summary>
    /// Represents the input data for a manual detection event.
    /// </summary>
    public class ManualDetectionInputModel
    {
        [Required]
        public string DeviceId { get; set; }
        
        [Required]
        [Display(Name = "Hornet Direction (degrees)")]
        public double HornetDirection { get; set; }
        
        [Required]
        [Display(Name = "Number of Hornets")]
        public int NumberOfHornets { get; set; }
        
        /// <summary>
        /// (Optional) Flight time in minutes.
        /// </summary>
        [Display(Name = "Flight Time (minutes)")]
        public double FlightTimeMinutes { get; set; }
    }
}