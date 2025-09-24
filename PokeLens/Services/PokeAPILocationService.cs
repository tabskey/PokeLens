using System.Text.Json;
using PokeLens.Models;
using PokeLens.Services.mappers;

namespace PokeLens.Services;

public class PokeLocationService : IPokeAPILocationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PokeLocationService> _logger;

    public PokeLocationService(HttpClient httpClient, ILogger<PokeLocationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

      public async Task<List<LocationArea>> GetLocationsByGenerationAsync(string pokemonName, string generation, string gameVersion,  int progress)
{
    try
    {
        _logger.LogInformation($"Searching for locations of {pokemonName} in {generation}");

        // Check if Pokemon exists
        var pokemonResponse = await _httpClient.GetAsync($"pokemon/{pokemonName.ToLower()}");
        if (!pokemonResponse.IsSuccessStatusCode)
            throw new Exception($"Pokemon {pokemonName} not found");

        // Busca todas as localizações do Pokémon
        var locationsResponse = await _httpClient.GetAsync($"pokemon/{pokemonName.ToLower()}/encounters");
        locationsResponse.EnsureSuccessStatusCode();

        var locationsContent = await locationsResponse.Content.ReadAsStringAsync();
        var allLocations = JsonSerializer.Deserialize<List<PokeLocation>>(locationsContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<PokeLocation>();

        // Jogos da geração
        var versionsInGeneration = GameGenerationMapper.GetGamesByGeneration(generation);
        var filteredVersions = versionsInGeneration
            .Where(v => v.Equals(gameVersion, StringComparison.OrdinalIgnoreCase))
            .ToHashSet();
        
        var tasks = allLocations.Select(async location =>
        {
            var hasGameEncounters = location.VersionDetails
                .Any(vd => filteredVersions.Contains(vd.Version.Name));

            var locationId = HeartGoldSoulSilverMapper.GetLocationId(location.LocationArea.Name);

            if (hasGameEncounters && locationId.HasValue && locationId.Value <= progress)
            {
                
                if (string.IsNullOrEmpty(location.LocationArea.DisplayName) && !string.IsNullOrEmpty(location.LocationArea.Url))
                {
                    try
                    {
                        var detailResponse = await _httpClient.GetAsync(location.LocationArea.Url);
                        if (detailResponse.IsSuccessStatusCode)
                        {
                            var detailContent = await detailResponse.Content.ReadAsStringAsync();
                            using var jsonDoc = JsonDocument.Parse(detailContent);
                            if (jsonDoc.RootElement.TryGetProperty("names", out var namesArray))
                            {
                                var englishName = namesArray.EnumerateArray()
                                    .FirstOrDefault(n => n.GetProperty("language").GetProperty("name").GetString() == "en")
                                    .GetProperty("name").GetString();

                                location.LocationArea.DisplayName = englishName ?? location.LocationArea.Name;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Could not fetch display_name for\n {location.LocationArea.Name}");
                        location.LocationArea.DisplayName = location.LocationArea.Name;
                    }
                }

                return location.LocationArea;
            }

            return null;
        });

        var filteredLocations = (await Task.WhenAll(tasks))
            .Where(loc => loc != null)
            .ToList();

        if (!filteredLocations.Any())
        {
            var generationNumber = GameGenerationMapper.GenerationNumberMap.TryGetValue(generation, out var number) 
                ? number 
                : generation;
            var capitalizedPokemonName = char.ToUpper(pokemonName[0]) + pokemonName.Substring(1).ToLower();
            throw new Exception($"Pokemon {capitalizedPokemonName} cannot be found in generation {generationNumber}");
        }

        return filteredLocations!;
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, $"Error searching for Pokemon {pokemonName} locations");
        throw new Exception($"Error communicating with PokeAPI: {ex.Message}");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error processing Pokemon {pokemonName} locations");
        throw;
    }
}


public async Task<List<LocationArea>> GetLocationsByGameAsync(string pokemonName, string gameName, string gameVersion, int progress)
    {
        var generation = GameGenerationMapper.GetGenerationByGame(gameName);
        return await GetLocationsByGenerationAsync(pokemonName, generation,gameVersion, progress );
    }

}
