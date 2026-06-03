using Microsoft.EntityFrameworkCore;
using TelemetryAPI.Models;

namespace TelemetryAPI.Data;

public class TelemetryDbContext : DbContext
{
    public TelemetryDbContext(DbContextOptions<TelemetryDbContext> options) : base(options) { }

    public DbSet<TelemetryReading> TelemetryReadings { get; set; }
    public DbSet<EnergyReading> EnergyReadings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TelemetryReading>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SensorId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.OperatorId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.BaseLocation).HasMaxLength(100);
            entity.HasIndex(e => e.OperatorId);
            entity.HasIndex(e => e.Timestamp);
        });

        modelBuilder.Entity<EnergyReading>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SensorId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.BaseLocation).HasMaxLength(100);
            entity.HasIndex(e => e.BaseLocation);
            entity.HasIndex(e => e.Timestamp);
        });
    }
}
