using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartApiary.Domain.Entities;

namespace SmartApiary.Infrastructure.Persistence.Configurations;

public class IoTDeviceConfiguration : IEntityTypeConfiguration<IoTDevice>
{
    public void Configure(EntityTypeBuilder<IoTDevice> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.SerialNumber).HasMaxLength(20).IsRequired();
        builder.HasIndex(d => d.SerialNumber).IsUnique();
        builder.HasIndex(d => d.UniqueDeviceId).IsUnique().HasFilter("[UniqueDeviceId] IS NOT NULL");
        builder.Property(d => d.Status).HasConversion<string>();
        builder.HasOne(d => d.Hive)
            .WithOne(h => h.IoTDevice)
            .HasForeignKey<IoTDevice>(d => d.HiveId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
