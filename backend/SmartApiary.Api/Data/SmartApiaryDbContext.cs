using Microsoft.EntityFrameworkCore;
using SmartApiary.Api.Models;

namespace SmartApiary.Api.Data;

public sealed class SmartApiaryDbContext(DbContextOptions<SmartApiaryDbContext> options) : DbContext(options)
{
    public DbSet<Telemetry> Telemetry => Set<Telemetry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Telemetry>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.HiveId).IsRequired().HasMaxLength(100);
            entity.HasIndex(x => new { x.HiveId, x.Timestamp });
        });
    }
}

