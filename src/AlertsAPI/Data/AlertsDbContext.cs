using AlertsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AlertsAPI.Data;

public class AlertsDbContext : DbContext
{
    public AlertsDbContext(DbContextOptions<AlertsDbContext> options) : base(options) { }

    public DbSet<Alert> Alerts { get; set; }
    public DbSet<EmergencyIncident> EmergencyIncidents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AlertId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SensorType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.BaseLocation).HasMaxLength(100);
            entity.Property(e => e.Severity).HasMaxLength(20);
            entity.HasIndex(e => e.AlertId).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ReceivedAt);
        });

        modelBuilder.Entity<EmergencyIncident>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AlertId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.IncidentTitle).IsRequired().HasMaxLength(100);
            entity.Property(e => e.BaseLocation).HasMaxLength(100);
        });
    }
}
