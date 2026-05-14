using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartApiary.Domain.Entities;

namespace SmartApiary.Infrastructure.Persistence.Configurations;

public class ParcelConfiguration : IEntityTypeConfiguration<Parcel>
{
    public void Configure(EntityTypeBuilder<Parcel> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.HasOne(p => p.Farmer)
            .WithMany(u => u.Parcels)
            .HasForeignKey(p => p.FarmerId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.CurrentCrop)
            .WithOne(c => c.Parcel)
            .HasForeignKey<Crop>(c => c.ParcelId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
