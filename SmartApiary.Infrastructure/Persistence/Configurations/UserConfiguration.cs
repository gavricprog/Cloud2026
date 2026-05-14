using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartApiary.Domain.Entities;

namespace SmartApiary.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Phone).HasMaxLength(20);
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.Role).HasConversion<string>();
        builder.Property(u => u.WeightDropThreshold).HasDefaultValue(10.0);
    }
}
