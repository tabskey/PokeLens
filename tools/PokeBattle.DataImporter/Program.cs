using System.Collections.Concurrent;
using System.Text.Json;

using PokeBattle.Application.GameData;
using PokeBattle.DataImporter;
using PokeBattle.DataImporter.PokeApi;

// Usage (from the repo root): dotnet run --project tools/PokeBattle.DataImporter [outputDir] [curatedProgression]
const int LastGen3Species = 386;
const int LastGen3Move = 354;

var outputDirectory = args.Length > 0 ? args[0] : Path.Combine("data", "gen3-frlg");
var curatedPath = args.Length > 1 ? args[1] : Path.Combine("data", "curated", "progression.json");

using var http = new HttpClient { BaseAddress = new Uri("https://pokeapi.co/api/v2/") };
http.DefaultRequestHeaders.UserAgent.ParseAdd("PokeBattle.DataImporter (+https://github.com/tabskey/PokeLens)");
var api = new PokeApiClient(http);

Console.WriteLine("Fetching version groups...");
var versionGroups = await api.GetAsync<ResourceList>("version-group/?limit=1000");
var versionGroupOrder = (await FetchAll(versionGroups.Results, g => api.GetAsync<VersionGroupResponse>(g.Url)))
    .ToDictionary(g => g.Name, g => g.Order);

Console.WriteLine("Fetching moves...");
var moves = (await FetchAll(Enumerable.Range(1, LastGen3Move), async id =>
    {
        var response = await api.GetAsync<MoveResponse>($"move/{id}/");
        var move = Gen3Normalizer.NormalizeMove(response, versionGroupOrder, machine: null);
        var machineUrl = response.Machines.FirstOrDefault(m => m.VersionGroup.Name == Gen3Normalizer.VersionGroup)?.Machine.Url;
        return move is null || machineUrl is null
            ? move
            : move with { Machine = (await api.GetAsync<MachineResponse>(machineUrl)).Item.Name };
    }))
    .OfType<MoveData>()
    .OrderBy(m => m.Id)
    .ToList();
var moveNames = moves.Select(m => m.Name).ToHashSet(StringComparer.Ordinal);

Console.WriteLine("Fetching species...");
var species = (await FetchAll(Enumerable.Range(1, LastGen3Species), async id =>
    {
        var pokemon = await api.GetAsync<PokemonResponse>($"pokemon/{id}/");
        var details = await api.GetAsync<SpeciesResponse>($"pokemon-species/{id}/");
        return Gen3Normalizer.NormalizePokemon(pokemon, details.CaptureRate, moveNames);
    }))
    .OrderBy(s => s.Id)
    .ToList();

Console.WriteLine("Fetching Kanto and Sevii encounters...");
var region = await api.GetAsync<RegionResponse>("region/kanto/");
var areas = (await FetchAll(region.Locations, l => api.GetAsync<LocationResponse>(l.Url))).SelectMany(l => l.Areas);
var encounters = (await FetchAll(areas, a => api.GetAsync<LocationAreaResponse>(a.Url)))
    .SelectMany(Gen3Normalizer.NormalizeEncounters)
    .OrderBy(e => e.Location, StringComparer.Ordinal)
    .ThenBy(e => e.Area, StringComparer.Ordinal)
    .ThenBy(e => e.Version, StringComparer.Ordinal)
    .ThenBy(e => e.Pokemon, StringComparer.Ordinal)
    .ThenBy(e => e.Method, StringComparer.Ordinal)
    .ThenBy(e => e.MinLevel)
    .ThenBy(e => e.MaxLevel)
    .ThenByDescending(e => e.Chance)
    .ThenBy(e => string.Join(',', e.Conditions), StringComparer.Ordinal)
    .ToList();

var progression = JsonSerializer.Deserialize<ProgressionData>(File.ReadAllText(curatedPath), GamePackage.JsonOptions)
    ?? throw new InvalidDataException($"'{curatedPath}' is empty.");

var package = new GamePackage(species, moves, encounters, progression);
var errors = package.Validate();
if (errors.Count > 0)
{
    Console.Error.WriteLine(string.Join(Environment.NewLine, errors));
    return 1;
}

Directory.CreateDirectory(outputDirectory);
Write(GamePackage.SpeciesFile, package.Species);
Write(GamePackage.MovesFile, package.Moves);
Write(GamePackage.EncountersFile, package.Encounters);
Write(GamePackage.ProgressionFile, package.Progression);
Console.WriteLine($"Wrote {species.Count} species, {moves.Count} moves and {encounters.Count} encounter slots to {outputDirectory}.");
return 0;

void Write<T>(string file, T value) =>
    File.WriteAllText(Path.Combine(outputDirectory, file), JsonSerializer.Serialize(value, GamePackage.JsonOptions) + "\n");

static async Task<List<TResult>> FetchAll<T, TResult>(IEnumerable<T> items, Func<T, Task<TResult>> fetch)
{
    var results = new ConcurrentBag<TResult>();
    await Parallel.ForEachAsync(items, new ParallelOptions { MaxDegreeOfParallelism = 8 }, async (item, _) => results.Add(await fetch(item)));
    return [.. results];
}