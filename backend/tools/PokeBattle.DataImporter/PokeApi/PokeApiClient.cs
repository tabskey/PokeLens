using System.Net.Http.Json;
using System.Text.Json;

namespace PokeBattle.DataImporter.PokeApi;

// shortcut: no retry or on-disk cache; the importer runs rarely, so a failed run is simply re-run.
internal sealed class PokeApiClient(HttpClient http)
{
    public static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    public async Task<T> GetAsync<T>(string path, CancellationToken cancellationToken = default) =>
        await http.GetFromJsonAsync<T>(path, JsonOptions, cancellationToken)
            ?? throw new InvalidDataException($"PokeAPI returned an empty body for '{path}'.");
}