using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartApiary.Domain.Entities;

namespace SmartApiary.Infrastructure.Persistence.Configurations;

public class ApiaryConfiguration : IEntityTypeConfiguration<Apiary>
{
    public void Configure(EntityTypeBuilder<Apiary> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).HasMaxLength(200).IsRequired();
        builder.HasOne(a => a.Beekeeper)
            .WithMany(u => u.Apiaries)
            .HasForeignKey(a => a.BeekeeperId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
