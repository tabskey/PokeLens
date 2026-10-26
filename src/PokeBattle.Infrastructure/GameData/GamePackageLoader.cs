using System.Text.Json;

using PokeBattle.Application.GameData;

namespace PokeBattle.Infrastructure.GameData;

/// <summary>Reads data/gen3-frlg into memory once. A missing or inconsistent package fails at startup.</summary>
public static class GamePackageLoader
{
    public static GamePackage Load(string directory)
    {
        var package = new GamePackage(
            Read<List<SpeciesData>>(directory, GamePackage.SpeciesFile),
            Read<List<MoveData>>(directory, GamePackage.MovesFile),
            Read<List<EncounterData>>(directory, GamePackage.EncountersFile),
            Read<ProgressionData>(directory, GamePackage.ProgressionFile));

        var errors = package.Validate();
        if (errors.Count > 0)
        {
            throw new InvalidDataException($"Game data package in '{directory}' is inconsistent:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }

        return package;
    }

    private static T Read<T>(string directory, string file)
    {
        using var stream = File.OpenRead(Path.Combine(directory, file));
        return JsonSerializer.Deserialize<T>(stream, GamePackage.JsonOptions)
            ?? throw new InvalidDataException($"Game data file '{file}' is empty.");
    }
}