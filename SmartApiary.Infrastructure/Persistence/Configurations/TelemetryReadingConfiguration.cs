using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartApiary.Domain.Entities;

namespace SmartApiary.Infrastructure.Persistence.Configurations;

public class TelemetryReadingConfiguration : IEntityTypeConfiguration<TelemetryReading>
{
    public void Configure(EntityTypeBuilder<TelemetryReading> builder)
    {
        builder.HasKey(t => t.Id);
        builder.HasIndex(t => new { t.HiveId, t.RecordedAt });
        builder.HasOne(t => t.Hive)
            .WithMany(h => h.TelemetryReadings)
            .HasForeignKey(t => t.HiveId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(t => t.Device)
            .WithMany(d => d.TelemetryReadings)
            .HasForeignKey(t => t.DeviceId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
