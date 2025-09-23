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
        _httpClient.BaseAddress = new Uri("https://pokeapi.co/api/v2/");
    }

    public async Task<List<LocationArea>> GetLocationsByGenerationAsync(string pokemonName, string generation)
    {
        try
        {
            _logger.LogInformation($"Searching for {pokemonName} locations in generation {generation}");

            var pokemonResponse = await _httpClient.GetAsync($"pokemon/{pokemonName.ToLower()}");
            if (!pokemonResponse.IsSuccessStatusCode) throw new Exception($"Pokemon {pokemonName} not found");

            // Get Pokemon locations
            var locationsResponse = await _httpClient.GetAsync($"pokemon/{pokemonName.ToLower()}/encounters");
            locationsResponse.EnsureSuccessStatusCode();

            var locationsContent = await locationsResponse.Content.ReadAsStringAsync();
            var allLocations = JsonSerializer.Deserialize<List<PokeLocation>>(locationsContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<PokeLocation>();

            // locations by generation
            var versionsInGeneration = GameGenerationMapper.GetGamesByGeneration(generation);
            var filteredLocations = new List<LocationArea>();

            foreach (var location in allLocations)
            {
                var hasGenerationEncounters = location.VersionDetails
                    .Any(vd => versionsInGeneration.Contains(vd.Version.Name));

                if (hasGenerationEncounters) filteredLocations.Add(location.LocationArea);
            }

            if (!filteredLocations.Any())
                throw new Exception($"Pokemon {pokemonName} cannot be found in generation {generation}");

            return filteredLocations;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, $"Error fetching locations for Pokemon {pokemonName}");
            throw new Exception($"Error communicating with PokeAPI: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing locations for Pokemon {pokemonName}");
            throw;
        }
    }

    public async Task<List<LocationArea>> GetLocationsByGameAsync(string pokemonName, string gameName)
    {
        var generation = GameGenerationMapper.GetGenerationByGame(gameName);
        return await GetLocationsByGenerationAsync(pokemonName, generation);
    }
}