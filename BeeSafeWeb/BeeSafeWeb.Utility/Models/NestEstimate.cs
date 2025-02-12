using System.ComponentModel.DataAnnotations.Schema;
using BeeSafeWeb.Utility.Misc;

namespace BeeSafeWeb.Utility.Models;

public class NestEstimate
{
    public Guid Id { get; set; }
    public double EstimatedLatitude { get; set; }
    public double EstimatedLongitude { get; set; }
    public double AccuracyLevel { get; set; }
    public bool IsDestroyed { get; set; }
    public DateTime Timestamp { get; set; }

    public KnownHornet? KnownHornet { get; set; }

    [NotMapped]
    public string LastUpdatedString => DateUtility.GetLastActiveString(Timestamp);
    
    // aggregated display properties:
    public double? DisplayLatitude { get; set; }
    public double? DisplayLongitude { get; set; }
    public double? DisplayAccuracy { get; set; }

    public double Direction { get; set; }

}