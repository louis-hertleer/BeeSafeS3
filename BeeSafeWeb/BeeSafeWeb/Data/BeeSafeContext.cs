using Microsoft.EntityFrameworkCore;
using BeeSafeWeb.Models;

namespace BeeSafeWeb.Data;

public class BeeSafeContext : DbContext
{
    public DbSet<ColorCode> ColorCodes { get; set; }
    public DbSet<DetectionEvent> DetectionEvents { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<KnownHornet> KnownHornets { get; set; }
    public DbSet<NestEstimate> NestEstimates { get; set; }
    public DbSet<User> Users { get; set; }
    
    public BeeSafeContext(DbContextOptions<BeeSafeContext> options) :
        base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ColorCode>().ToTable("ColorCode");
        modelBuilder.Entity<DetectionEvent>().ToTable("DetectionEvent");
        modelBuilder.Entity<Device>().ToTable("Device");
        modelBuilder.Entity<KnownHornet>().ToTable("KnownHornet");
        modelBuilder.Entity<NestEstimate>().ToTable("NestEstimate");
        modelBuilder.Entity<User>().ToTable("User");
        
        base.OnModelCreating(modelBuilder);
    }
}