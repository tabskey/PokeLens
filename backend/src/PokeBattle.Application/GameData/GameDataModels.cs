using PokeBattle.Core.Battles;
using PokeBattle.Core.Types;

namespace PokeBattle.Application.GameData;

/// <summary>A Pokémon as it was in FireRed/LeafGreen: Gen III types, base stats and non-hidden abilities.</summary>
public sealed record SpeciesData(
    int Id,
    string Name,
    IReadOnlyList<PokemonType> Types,
    StatSpread BaseStats,
    IReadOnlyList<string> Abilities,
    int CaptureRate,
    IReadOnlyList<LearnsetEntry> Learnset);

/// <summary>Level is the level-up level; 0 for every other method.</summary>
public sealed record LearnsetEntry(string Move, LearnMethod Method, int Level);

public enum LearnMethod
{
    LevelUp,
    Machine,
    Egg,
    Tutor,
}

/// <summary>
/// A damaging move with a fixed base power, as it was in Gen III. Status moves and moves whose
/// power is variable (Return, Seismic Toss, Low Kick...) are not part of the package.
/// </summary>
public sealed record MoveData(
    int Id,
    string Name,
    PokemonType Type,
    MoveCategory Category,
    int Power,
    int? Accuracy,
    int Pp,
    bool HighCriticalRatio,
    string? Machine);

/// <summary>One encounter slot of a location area, per game version (method is the PokeAPI name: walk, surf, gift...).</summary>
public sealed record EncounterData(
    string Location,
    string Area,
    string Version,
    string Pokemon,
    string Method,
    int MinLevel,
    int MaxLevel,
    int Chance,
    IReadOnlyList<string> Conditions);

/// <summary>
/// Hand-curated progression. Milestones are in game order; an area, encounter method or machine is
/// available from its milestone on. Anything not listed is treated as not yet available.
/// </summary>
public sealed record ProgressionData(
    IReadOnlyList<string> Milestones,
    IReadOnlyList<Unlock> EncounterMethods,
    IReadOnlyList<AreaUnlock> Areas,
    IReadOnlyList<Unlock> Machines)
{
    /// <summary>Names of the unlocks already available at <paramref name="milestone"/>.</summary>
    public HashSet<string> UnlockedAt(IEnumerable<Unlock> unlocks, string milestone)
    {
        var order = Milestones.ToList();
        var reached = order.IndexOf(milestone);
        return unlocks.Where(u => order.IndexOf(u.Milestone) <= reached).Select(u => u.Name).ToHashSet(StringComparer.Ordinal);
    }
}

public record Unlock(string Name, string Milestone);

/// <param name="DisplayName">Name shown in the target search (e.g. "Route 2 (South)").</param>
public sealed record AreaUnlock(string Name, string Milestone, string DisplayName) : Unlock(Name, Milestone);

/// <summary>Milestones in game order, for the progression picker. Areas come with the targets (GET /targets).</summary>
public sealed record GameOptions(IReadOnlyList<string> Milestones);