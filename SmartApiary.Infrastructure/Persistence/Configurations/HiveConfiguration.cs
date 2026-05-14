using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartApiary.Domain.Entities;

namespace SmartApiary.Infrastructure.Persistence.Configurations;

public class HiveConfiguration : IEntityTypeConfiguration<Hive>
{
    public void Configure(EntityTypeBuilder<Hive> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Label).HasMaxLength(100).IsRequired();
        builder.Property(h => h.HiveType).HasConversion<string>();
        builder.Property(h => h.FrameColor).HasMaxLength(50);
        builder.HasOne(h => h.Apiary)
            .WithMany(a => a.Hives)
            .HasForeignKey(h => h.ApiaryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
