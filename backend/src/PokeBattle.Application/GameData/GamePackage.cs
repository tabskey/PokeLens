using System.Text.Json;
using System.Text.Json.Serialization;

namespace PokeBattle.Application.GameData;

/// <summary>The Gen III FireRed/LeafGreen data package (data/gen3-frlg), written by the DataImporter.</summary>
public sealed record GamePackage(
    IReadOnlyList<SpeciesData> Species,
    IReadOnlyList<MoveData> Moves,
    IReadOnlyList<EncounterData> Encounters,
    ProgressionData Progression)
{
    public const string SpeciesFile = "species.json";
    public const string MovesFile = "moves.json";
    public const string EncountersFile = "encounters.json";
    public const string ProgressionFile = "progression.json";

    public static readonly IReadOnlyList<string> Versions = ["firered", "leafgreen"];

    /// <summary>
    /// Methods a target can be met with in V1. Gift, static and trades already come in the package
    /// but are left out until they are offered as targets (docs/arquitetura.md §7.1).
    /// </summary>
    public static readonly IReadOnlySet<string> WildMethods =
        new HashSet<string>(["walk", "surf", "old-rod", "good-rod", "super-rod", "rock-smash"], StringComparer.Ordinal);

    /// <summary>Shared by the importer and the loader so the files round-trip byte for byte.</summary>
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        NewLine = "\n",
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>Errors for an unknown version or milestone, keyed by request field; empty when both are valid.</summary>
    public Dictionary<string, string[]> ValidateProgress(string? version, string? milestone)
    {
        var errors = new Dictionary<string, string[]>();
        if (version is null || !Versions.Contains(version))
        {
            errors["version"] = ["Versão deve ser 'firered' ou 'leafgreen'."];
        }

        if (milestone is null || !Progression.Milestones.Contains(milestone))
        {
            errors["milestone"] = [$"Ponto de progressão desconhecido. Valores aceitos: {string.Join(", ", Progression.Milestones)}."];
        }

        return errors;
    }

    /// <summary>Encounters of the version whose area and method are both unlocked at the milestone.</summary>
    public IEnumerable<EncounterData> UnlockedEncounters(string version, string milestone)
    {
        var areas = Progression.UnlockedAt(Progression.Areas, milestone);
        var methods = Progression.UnlockedAt(Progression.EncounterMethods, milestone);
        return Encounters.Where(e => e.Version == version && areas.Contains(e.Area) && methods.Contains(e.Method));
    }

    /// <summary>Every cross-reference must resolve; an empty list means the package is consistent.</summary>
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        var moveNames = Moves.Select(m => m.Name).ToHashSet(StringComparer.Ordinal);
        var speciesNames = Species.Select(s => s.Name).ToHashSet(StringComparer.Ordinal);
        var areaNames = Encounters.Select(e => e.Area).ToHashSet(StringComparer.Ordinal);
        var methodNames = Encounters.Select(e => e.Method).ToHashSet(StringComparer.Ordinal);
        var machineNames = Moves.Where(m => m.Machine is not null).Select(m => m.Machine!).ToHashSet(StringComparer.Ordinal);
        var milestones = Progression.Milestones.ToHashSet(StringComparer.Ordinal);

        errors.AddRange(Species.SelectMany(s => s.Learnset
            .Where(l => !moveNames.Contains(l.Move))
            .Select(l => $"Species '{s.Name}' learns unknown move '{l.Move}'.")));
        errors.AddRange(Encounters
            .Where(e => !speciesNames.Contains(e.Pokemon))
            .Select(e => $"Area '{e.Area}' has unknown Pokémon '{e.Pokemon}'."));

        if (milestones.Count != Progression.Milestones.Count)
        {
            errors.Add("Progression milestones must be unique.");
        }

        CheckUnlocks("encounter method", Progression.EncounterMethods, methodNames);
        CheckUnlocks("area", Progression.Areas, areaNames);
        CheckUnlocks("machine", Progression.Machines, machineNames);
        errors.AddRange(Progression.Areas
            .Where(a => string.IsNullOrWhiteSpace(a.DisplayName))
            .Select(a => $"Progression area '{a.Name}' has no displayName."));
        return errors;

        void CheckUnlocks(string kind, IReadOnlyList<Unlock> unlocks, HashSet<string> known)
        {
            foreach (var unlock in unlocks)
            {
                if (!known.Contains(unlock.Name))
                {
                    errors.Add($"Progression {kind} '{unlock.Name}' does not exist in the package.");
                }

                if (!milestones.Contains(unlock.Milestone))
                {
                    errors.Add($"Progression {kind} '{unlock.Name}' references unknown milestone '{unlock.Milestone}'.");
                }
            }

            if (unlocks.Select(u => u.Name).Distinct(StringComparer.Ordinal).Count() != unlocks.Count)
            {
                errors.Add($"Progression {kind} entries must be unique.");
            }
        }
    }
}