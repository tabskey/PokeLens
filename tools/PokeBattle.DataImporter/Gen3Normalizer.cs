using PokeBattle.Application.GameData;
using PokeBattle.Core.Battles;
using PokeBattle.Core.Types;
using PokeBattle.DataImporter.PokeApi;

namespace PokeBattle.DataImporter;

/// <summary>
/// Turns modern PokeAPI data into what FireRed/LeafGreen actually had. PokeAPI "past_*" entries hold the
/// values a resource had up to and including the given generation (or version group), so the entry that
/// applies is the earliest one at or after Gen III / FRLG; with none, the current value is the Gen III one.
/// </summary>
internal static class Gen3Normalizer
{
    public const int Generation = 3;
    public const string VersionGroup = "firered-leafgreen";
    public static readonly string[] Versions = ["firered", "leafgreen"];

    /// <param name="versionGroupOrder">PokeAPI "order" of each version group, to place past_values in time.</param>
    /// <returns>Null when the move has no fixed base power in Gen III (status or variable power).</returns>
    public static MoveData? NormalizeMove(MoveResponse move, IReadOnlyDictionary<string, int> versionGroupOrder, string? machine)
    {
        var frlgOrder = versionGroupOrder[VersionGroup];
        var past = move.PastValues
            .Where(p => versionGroupOrder[p.VersionGroup.Name] >= frlgOrder)
            .OrderBy(p => versionGroupOrder[p.VersionGroup.Name])
            .ToList();

        // A null field in a past entry means "unchanged", so each field takes the first non-null value.
        var power = past.Select(p => p.Power).FirstOrDefault(v => v is not null) ?? move.Power;
        // PokeAPI stores power 1 for variable power (Hidden Power through Gen VI).
        if (power is null or <= 1)
        {
            return null;
        }

        var type = ParseType((past.Select(p => p.Type).FirstOrDefault(t => t is not null) ?? move.Type).Name);
        return new MoveData(
            move.Id,
            move.Name,
            type,
            // Never PokeAPI damage_class: in Gen III the category follows the type.
            MoveCategoryResolver.GetCategory(type),
            power.Value,
            past.Select(p => p.Accuracy).FirstOrDefault(v => v is not null) ?? move.Accuracy,
            past.Select(p => p.Pp).FirstOrDefault(v => v is not null) ?? move.Pp ?? 0,
            move.Meta?.CritRate > 0,
            machine);
    }

    /// <param name="knownMoves">Moves in the package; learnset entries for anything else are dropped.</param>
    public static SpeciesData NormalizePokemon(PokemonResponse pokemon, int captureRate, IReadOnlySet<string> knownMoves)
    {
        var types = (FromGeneration(pokemon.PastTypes, p => p.Generation)?.Types ?? pokemon.Types)
            .OrderBy(t => t.Slot)
            .Select(t => ParseType(t.Type.Name))
            .ToList();

        return new SpeciesData(
            pokemon.Id,
            pokemon.Name,
            types,
            BaseStats(pokemon),
            Abilities(pokemon),
            captureRate,
            Learnset(pokemon, knownMoves));
    }

    public static IEnumerable<EncounterData> NormalizeEncounters(LocationAreaResponse area) =>
        from pokemon in area.PokemonEncounters
        from version in pokemon.VersionDetails
        where Versions.Contains(version.Version.Name)
        from slot in version.EncounterDetails
        select new EncounterData(
            area.Location.Name,
            area.Name,
            version.Version.Name,
            pokemon.Pokemon.Name,
            slot.Method.Name,
            slot.MinLevel,
            slot.MaxLevel,
            slot.Chance,
            slot.ConditionValues.Select(c => c.Name).Order(StringComparer.Ordinal).ToList());

    private static StatSpread BaseStats(PokemonResponse pokemon)
    {
        int Stat(string name) =>
            FromGeneration(pokemon.PastStats.Where(p => p.Stats.Any(s => s.Stat.Name == name)), p => p.Generation)?
                .Stats.Single(s => s.Stat.Name == name).BaseStat
            ?? pokemon.Stats.Single(s => s.Stat.Name == name).BaseStat;

        return new StatSpread(Stat("hp"), Stat("attack"), Stat("defense"), Stat("special-attack"), Stat("special-defense"), Stat("speed"));
    }

    private static List<string> Abilities(PokemonResponse pokemon)
    {
        var slots = pokemon.Abilities.ToDictionary(a => a.Slot);
        foreach (var slot in pokemon.PastAbilities.SelectMany(p => p.Abilities).Select(a => a.Slot).Distinct())
        {
            var past = FromGeneration(pokemon.PastAbilities.Where(p => p.Abilities.Any(a => a.Slot == slot)), p => p.Generation);
            if (past is not null)
            {
                slots[slot] = past.Abilities.Single(a => a.Slot == slot);
            }
        }

        // Hidden abilities did not exist in Gen III; a null past ability means the slot was empty.
        return slots.Values
            .Where(a => !a.IsHidden && a.Ability is not null)
            .OrderBy(a => a.Slot)
            .Select(a => a.Ability!.Name)
            .ToList();
    }

    private static List<LearnsetEntry> Learnset(PokemonResponse pokemon, IReadOnlySet<string> knownMoves) =>
        pokemon.Moves
            .Where(m => knownMoves.Contains(m.Move.Name))
            .SelectMany(m => m.VersionGroupDetails
                .Where(d => d.VersionGroup.Name == VersionGroup)
                .Select(d => new LearnsetEntry(m.Move.Name, ParseLearnMethod(d.MoveLearnMethod.Name), d.LevelLearnedAt)))
            .Distinct()
            .OrderBy(l => l.Method)
            .ThenBy(l => l.Level)
            .ThenBy(l => l.Move, StringComparer.Ordinal)
            .ToList();

    private static T? FromGeneration<T>(IEnumerable<T> past, Func<T, NamedResource> generation) where T : class =>
        past.Where(p => generation(p).Id >= Generation).MinBy(p => generation(p).Id);

    private static PokemonType ParseType(string name) => Enum.Parse<PokemonType>(name, ignoreCase: true);

    private static LearnMethod ParseLearnMethod(string name) => name switch
    {
        "level-up" => LearnMethod.LevelUp,
        "machine" => LearnMethod.Machine,
        "egg" => LearnMethod.Egg,
        "tutor" => LearnMethod.Tutor,
        _ => throw new InvalidDataException($"Unknown learn method '{name}' in {VersionGroup}."),
    };
}