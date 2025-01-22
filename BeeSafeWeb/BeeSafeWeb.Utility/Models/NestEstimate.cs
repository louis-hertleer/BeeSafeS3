using System.ComponentModel.DataAnnotations.Schema;

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
    public string LastUpdatedString
    {
        get
        {
            var date = DateTime.Now - Timestamp;
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

}