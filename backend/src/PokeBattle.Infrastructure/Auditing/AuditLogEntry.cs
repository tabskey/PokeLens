using PokeBattle.Core.Battles;

namespace PokeBattle.Infrastructure.Auditing;

/// <summary>One row of the AuditLog table; mirrors <see cref="Application.Auditing.AuditEntry"/>.</summary>
public sealed class AuditLogEntry
{
    public long Id { get; set; }

    public required string Event { get; set; }

    public string? UserId { get; set; }

    public required string Target { get; set; }

    public required string Area { get; set; }

    public required string Method { get; set; }

    public required string Version { get; set; }

    public required string Milestone { get; set; }

    public BattleObjective Objective { get; set; }

    /// <summary>Stored as a Postgres text[].</summary>
    public required List<string> Candidates { get; set; }

    public DateTimeOffset OccurredAt { get; set; }
}