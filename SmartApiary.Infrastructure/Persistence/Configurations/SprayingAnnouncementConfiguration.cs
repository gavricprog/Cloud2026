using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartApiary.Domain.Entities;

namespace SmartApiary.Infrastructure.Persistence.Configurations;

public class SprayingAnnouncementConfiguration : IEntityTypeConfiguration<SprayingAnnouncement>
{
    public void Configure(EntityTypeBuilder<SprayingAnnouncement> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Status).HasConversion<string>();
        builder.Property(s => s.SubstanceType).HasMaxLength(200);
        builder.HasOne(s => s.Parcel)
            .WithMany(p => p.SprayingAnnouncements)
            .HasForeignKey(s => s.ParcelId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(s => s.SprayingLog)
            .WithOne(l => l.SprayingAnnouncement)
            .HasForeignKey<SprayingLog>(l => l.SprayingAnnouncementId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
