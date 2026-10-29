using PokeBattle.Core.Battles;

namespace PokeBattle.Infrastructure.Favorites;

/// <summary>One row of the SavedPrediction table; mirrors <see cref="Application.Favorites.StoredFavorite"/>.</summary>
public sealed class SavedPrediction
{
    public long Id { get; set; }

    public required string UserId { get; set; }

    public string? Name { get; set; }

    public required string Target { get; set; }

    public required string Area { get; set; }

    public required string Method { get; set; }

    public required string Version { get; set; }

    public required string Milestone { get; set; }

    public BattleObjective Objective { get; set; }

    public ScenarioKind Scenario { get; set; }

    /// <summary>Candidate names when saved, only to describe the favorite; stored as a Postgres text[].</summary>
    public required List<string> Candidates { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}