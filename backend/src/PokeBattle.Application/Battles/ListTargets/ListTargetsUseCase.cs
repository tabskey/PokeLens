using PokeBattle.Application.GameData;

namespace PokeBattle.Application.Battles.ListTargets;

/// <summary>
/// The targets the player can pick at a milestone (docs/arquitetura.md §7.1): only wild encounters whose
/// area and method are both unlocked, one option per (area, method) pair. The frontend searches this list.
/// </summary>
public sealed class ListTargetsUseCase(GamePackage package)
{
    public IReadOnlyDictionary<string, string[]> Validate(string? version, string? milestone)
        => package.ValidateProgress(version, milestone);

    /// <exception cref="ArgumentException">The version or milestone is invalid; call <see cref="Validate"/> first.</exception>
    public IReadOnlyList<TargetOption> Execute(string version, string milestone)
    {
        if (Validate(version, milestone).Count > 0)
        {
            throw new ArgumentException("Invalid version or milestone; call Validate first.", nameof(milestone));
        }

        var ids = package.Species.ToDictionary(s => s.Name, s => s.Id, StringComparer.Ordinal);
        var areas = package.Progression.Areas.ToList();

        return [.. package.UnlockedEncounters(version, milestone)
            .Where(e => GamePackage.WildMethods.Contains(e.Method))
            .GroupBy(e => e.Pokemon, StringComparer.Ordinal)
            .OrderBy(species => ids[species.Key])
            .Select(species => new TargetOption(species.Key, ids[species.Key], [.. species
                .GroupBy(e => (e.Area, e.Method))
                .Select(slots => (Index: areas.FindIndex(a => a.Name == slots.Key.Area), Slots: slots))
                .OrderBy(pair => pair.Index)
                .ThenBy(pair => pair.Slots.Key.Method, StringComparer.Ordinal)
                .Select(pair => new TargetEncounter(
                    pair.Slots.Key.Area,
                    areas[pair.Index].DisplayName,
                    pair.Slots.Key.Method,
                    pair.Slots.Min(e => e.MinLevel),
                    pair.Slots.Max(e => e.MaxLevel),
                    pair.Slots.Sum(e => e.Chance)))]))];
    }
}