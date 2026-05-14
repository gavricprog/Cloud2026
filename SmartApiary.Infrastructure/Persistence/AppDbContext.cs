using Microsoft.EntityFrameworkCore;
using SmartApiary.Domain.Entities;

namespace SmartApiary.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Apiary> Apiaries => Set<Apiary>();
    public DbSet<Hive> Hives => Set<Hive>();
    public DbSet<IoTDevice> IoTDevices => Set<IoTDevice>();
    public DbSet<TelemetryReading> TelemetryReadings => Set<TelemetryReading>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<HiveDiaryEntry> HiveDiaryEntries => Set<HiveDiaryEntry>();
    public DbSet<Parcel> Parcels => Set<Parcel>();
    public DbSet<Crop> Crops => Set<Crop>();
    public DbSet<SprayingAnnouncement> SprayingAnnouncements => Set<SprayingAnnouncement>();
    public DbSet<SprayingLog> SprayingLogs => Set<SprayingLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
