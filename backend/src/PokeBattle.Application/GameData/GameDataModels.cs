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
    IReadOnlyList<Unlock> Areas,
    IReadOnlyList<Unlock> Machines);

public sealed record Unlock(string Name, string Milestone);