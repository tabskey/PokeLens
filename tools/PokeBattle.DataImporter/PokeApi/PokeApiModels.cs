namespace PokeBattle.DataImporter.PokeApi;

// Only the PokeAPI fields the importer reads. Property names map from snake_case (see PokeApiClient).

internal sealed record NamedResource(string Name, string Url)
{
    /// <summary>The numeric id at the end of the resource URL (".../generation/3/" -> 3).</summary>
    public int Id => int.Parse(Url.TrimEnd('/').Split('/')[^1], System.Globalization.CultureInfo.InvariantCulture);
}

internal sealed record ResourceList(List<NamedResource> Results);

internal sealed record PokemonResponse(
    int Id,
    string Name,
    List<TypeSlot> Types,
    List<PastTypes> PastTypes,
    List<StatEntry> Stats,
    List<PastStats> PastStats,
    List<AbilitySlot> Abilities,
    List<PastAbilities> PastAbilities,
    List<PokemonMove> Moves);

internal sealed record TypeSlot(int Slot, NamedResource Type);

internal sealed record PastTypes(NamedResource Generation, List<TypeSlot> Types);

internal sealed record StatEntry(int BaseStat, NamedResource Stat);

internal sealed record PastStats(NamedResource Generation, List<StatEntry> Stats);

internal sealed record AbilitySlot(bool IsHidden, int Slot, NamedResource? Ability);

internal sealed record PastAbilities(NamedResource Generation, List<AbilitySlot> Abilities);

internal sealed record PokemonMove(NamedResource Move, List<VersionGroupDetail> VersionGroupDetails);

internal sealed record VersionGroupDetail(int LevelLearnedAt, NamedResource MoveLearnMethod, NamedResource VersionGroup);

internal sealed record SpeciesResponse(int CaptureRate);

internal sealed record MoveResponse(
    int Id,
    string Name,
    int? Power,
    int? Accuracy,
    int? Pp,
    NamedResource Type,
    List<PastMoveValues> PastValues,
    MoveMeta? Meta,
    List<MachineEntry> Machines);

internal sealed record PastMoveValues(int? Power, int? Accuracy, int? Pp, NamedResource? Type, NamedResource VersionGroup);

internal sealed record MoveMeta(int CritRate);

internal sealed record MachineEntry(NamedResource Machine, NamedResource VersionGroup);

internal sealed record MachineResponse(NamedResource Item);

internal sealed record VersionGroupResponse(string Name, int Order);

internal sealed record RegionResponse(List<NamedResource> Locations);

internal sealed record LocationResponse(List<NamedResource> Areas);

internal sealed record LocationAreaResponse(string Name, NamedResource Location, List<PokemonEncounter> PokemonEncounters);

internal sealed record PokemonEncounter(NamedResource Pokemon, List<VersionEncounters> VersionDetails);

internal sealed record VersionEncounters(NamedResource Version, List<EncounterDetail> EncounterDetails);

internal sealed record EncounterDetail(int MinLevel, int MaxLevel, int Chance, List<NamedResource> ConditionValues, NamedResource Method);