using System;
using System.ComponentModel.DataAnnotations;

namespace BeeSafeWeb.Models
{
    /// <summary>
    /// Represents the input data for a manual detection event.
    /// </summary>
    public class ManualDetectionInputModel
    {
        [Required(ErrorMessage = "Device ID is required.")]
        public string DeviceId { get; set; }
        
        [Required(ErrorMessage = "Hornet direction is required.")]
        [Range(0, 360, ErrorMessage = "Hornet direction must be between 0 and 360 degrees.")]
        [Display(Name = "Hornet Direction (degrees)")]
        public double HornetDirection { get; set; }
        
        [Required(ErrorMessage = "Number of hornets is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Number of hornets must be at least 1.")]
        [Display(Name = "Number of Hornets")]
        public int NumberOfHornets { get; set; }
           
        [Required(ErrorMessage = "Flight time is required.")]
        [Range(0.1, double.MaxValue, ErrorMessage = "Flight time must be at least 0.1 minutes.")]
        [Display(Name = "Flight Time (minutes)")]
        public double FlightTimeMinutes { get; set; }
    }
}