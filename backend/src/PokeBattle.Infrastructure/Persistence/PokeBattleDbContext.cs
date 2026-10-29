using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using PokeBattle.Infrastructure.Auditing;

namespace PokeBattle.Infrastructure.Persistence;

/// <summary>Users (ASP.NET Identity) and the audit log. Game data never lives here; it is the static package.</summary>
public sealed class PokeBattleDbContext(DbContextOptions<PokeBattleDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
    public DbSet<AuditLogEntry> AuditLog => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AuditLogEntry>(entry =>
        {
            entry.ToTable("AuditLog");
            entry.Property(e => e.Event).HasMaxLength(64);
            entry.Property(e => e.UserId).HasMaxLength(450);
            entry.Property(e => e.Target).HasMaxLength(64);
            entry.Property(e => e.Area).HasMaxLength(128);
            entry.Property(e => e.Method).HasMaxLength(32);
            entry.Property(e => e.Version).HasMaxLength(16);
            entry.Property(e => e.Milestone).HasMaxLength(64);
            entry.Property(e => e.Objective).HasConversion<string>().HasMaxLength(16);
            entry.HasIndex(e => e.OccurredAt);
            entry.HasIndex(e => e.UserId);
        });
    }
}