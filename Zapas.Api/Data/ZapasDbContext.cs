using Microsoft.EntityFrameworkCore;
using Zapas.Api.Entities;

namespace Zapas.Api.Data;

public class ZapasDbContext : DbContext
{
    public ZapasDbContext(DbContextOptions<ZapasDbContext> options) : base(options)
    {
    }

    public DbSet<SessionEntity> Sessions => Set<SessionEntity>();
    public DbSet<RunIntervalEntity> RunIntervals => Set<RunIntervalEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SessionEntity>(entity =>
        {
            entity.ToTable("Sessions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OwnerUserId).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.StartTime)
                  .HasConversion(
                      value => value.ToUnixTimeMilliseconds(),
                      value => DateTimeOffset.FromUnixTimeMilliseconds(value))
                  .IsRequired();
            entity.Property(e => e.CreatedAtUtc)
                  .HasConversion(
                      value => value.ToUnixTimeMilliseconds(),
                      value => DateTimeOffset.FromUnixTimeMilliseconds(value))
                  .IsRequired();
            entity.Property(e => e.AveragePaceSecondsPerKm).IsRequired();
            entity.HasMany(e => e.Intervals)
                  .WithOne(i => i.Session)
                  .HasForeignKey(i => i.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.StartTime);
            entity.HasIndex(e => e.OwnerUserId);
        });

        modelBuilder.Entity<RunIntervalEntity>(entity =>
        {
            entity.ToTable("RunIntervals");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LapNumber).IsRequired();
            entity.HasIndex(x => x.SessionId);
        });
    }
}
