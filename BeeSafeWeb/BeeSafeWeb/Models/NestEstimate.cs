namespace BeeSafeWeb.Models;

public class NestEstimate
{
    public Guid Id { get; set; }
    public double EstimatedLatitude { get; set; }
    public double EstimatedLongitude { get; set; }
    public double AccuracyLevel { get; set; }
    public bool IsDestroyed { get; set;}
    public DateTime Timestamp { get; set; }
    
    public KnownHornet? KnownHornet { get; set; }
}