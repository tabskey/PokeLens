using PokeBattle.Core.Battles;

namespace PokeBattle.Application.Auditing;

/// <summary>Where the Application records what players do, to understand usage and find problems. The Core knows nothing about it.</summary>
public interface IAuditLog
{
    Task RecordAsync(AuditEntry entry, CancellationToken cancellationToken);
}

public static class AuditEvents
{
    public const string BattlePredictionRequested = nameof(BattlePredictionRequested);
    public const string BattlePredictionCompleted = nameof(BattlePredictionCompleted);
}

/// <param name="UserId">Null for anonymous players: logging in is optional.</param>
/// <param name="Candidates">Recommended Pokémon in ranking order; empty for the request.</param>
public sealed record AuditEntry(
    string Event,
    string? UserId,
    string Target,
    string Area,
    string Method,
    string Version,
    string Milestone,
    BattleObjective Objective,
    IReadOnlyList<string> Candidates,
    DateTimeOffset OccurredAt);