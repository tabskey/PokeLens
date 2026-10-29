using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using PokeBattle.Infrastructure.Auditing;
using PokeBattle.Infrastructure.Favorites;

namespace PokeBattle.Infrastructure.Persistence;

/// <summary>Users (ASP.NET Identity) and the audit log. Game data never lives here; it is the static package.</summary>
public sealed class PokeBattleDbContext(DbContextOptions<PokeBattleDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
    /// <summary>The only role for now; who has it comes from Auth:AdminEmails, synced at login.</summary>
    public const string AdminRole = "Admin";

    public DbSet<AuditLogEntry> AuditLog => Set<AuditLogEntry>();

    public DbSet<SavedPrediction> SavedPredictions => Set<SavedPrediction>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Fixed id and stamp so the seed does not change on every migration.
        builder.Entity<IdentityRole>().HasData(new IdentityRole
        {
            Id = "6f0b1a2e-3c4d-4e5f-8a9b-0c1d2e3f4a5b",
            Name = AdminRole,
            NormalizedName = AdminRole.ToUpperInvariant(),
            ConcurrencyStamp = "a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d",
        });

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

        builder.Entity<SavedPrediction>(favorite =>
        {
            favorite.ToTable("SavedPrediction");
            favorite.Property(f => f.Name).HasMaxLength(60);
            favorite.Property(f => f.Target).HasMaxLength(64);
            favorite.Property(f => f.Area).HasMaxLength(128);
            favorite.Property(f => f.Method).HasMaxLength(32);
            favorite.Property(f => f.Version).HasMaxLength(16);
            favorite.Property(f => f.Milestone).HasMaxLength(64);
            favorite.Property(f => f.Objective).HasConversion<string>().HasMaxLength(16);
            favorite.Property(f => f.Scenario).HasConversion<string>().HasMaxLength(16);
            // Deleting an account deletes its favorites.
            favorite.HasOne<IdentityUser>().WithMany().HasForeignKey(f => f.UserId).OnDelete(DeleteBehavior.Cascade);
            favorite.HasIndex(f => new { f.UserId, f.CreatedAt });
            // One favorite per question and user; saving it again returns the existing one.
            favorite.HasIndex(f => new { f.UserId, f.Target, f.Area, f.Method, f.Version, f.Milestone, f.Objective, f.Scenario }).IsUnique();
        });
    }
}