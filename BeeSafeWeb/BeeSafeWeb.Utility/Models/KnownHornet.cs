namespace BeeSafeWeb.Utility.Models;

public class KnownHornet
{
    public Guid Id { get; set; }
    
    public List<NestEstimate> NestEstimates { get; set; }
}