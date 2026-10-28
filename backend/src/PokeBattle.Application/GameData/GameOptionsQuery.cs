namespace PokeBattle.Application.GameData;

/// <param name="Milestone">Progression milestone that unlocks the area; null when the area is not curated yet.</param>
public sealed record AreaOption(string Area, string? Milestone, IReadOnlyList<string> Pokemon);

public sealed record GameOptions(IReadOnlyList<string> Milestones, IReadOnlyList<AreaOption> Areas);

/// <summary>What the frontend needs to build the prediction form for one game version.</summary>
public sealed class GameOptionsQuery(GamePackage package)
{
    public GameOptions Execute(string version)
    {
        var milestones = package.Progression.Areas.ToDictionary(a => a.Name, a => a.Milestone, StringComparer.Ordinal);

        var areas = package.Encounters
            .Where(e => e.Version == version)
            .GroupBy(e => e.Area, StringComparer.Ordinal)
            .Select(group => new AreaOption(
                group.Key,
                milestones.GetValueOrDefault(group.Key),
                [.. group.Select(e => e.Pokemon).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)]))
            .OrderBy(area => area.Area, StringComparer.Ordinal)
            .ToList();

        return new GameOptions(package.Progression.Milestones, areas);
    }
}