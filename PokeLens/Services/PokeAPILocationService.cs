using PokeLens.Models;
using PokeLens.Services.mappers;
using System.Text.Json;

namespace PokeLens.Services;

public class PokeLocationService : IPokeAPILocationService
{    private readonly HttpClient _httpClient;
    private readonly ILogger<PokeLocationService> _logger;
    
    public PokeLocationService(HttpClient httpClient, ILogger<PokeLocationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.BaseAddress = new Uri("https://pokeapi.co/api/v2/");
    }
  public async Task<List<LocationArea>> GetLocationsByGenerationAsync(string pokemonName, string generation, string gameVersion,  int progress)
{
    try
    {
        _logger.LogInformation($"Buscando localizações do {pokemonName} na {generation}");

        // Verifica se o Pokémon existe
        var pokemonResponse = await _httpClient.GetAsync($"pokemon/{pokemonName.ToLower()}");
        if (!pokemonResponse.IsSuccessStatusCode)
            throw new Exception($"Pokémon {pokemonName} não encontrado");

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
                        _logger.LogWarning(ex, $"Não foi possível buscar display_name para {location.LocationArea.Name}");
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
            throw new Exception($"O Pokémon {pokemonName} não pode ser encontrado na geração {generation}");

        return filteredLocations!;
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, $"Erro ao buscar localizações do Pokémon {pokemonName}");
        throw new Exception($"Erro ao comunicar com a PokeAPI: {ex.Message}");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Erro ao processar localizações do Pokémon {pokemonName}");
        throw;
    }
}


public async Task<List<LocationArea>> GetLocationsByGameAsync(string pokemonName, string gameName, string gameVersion, int progress)
    {
        var generation = GameGenerationMapper.GetGenerationByGame(gameName);
        return await GetLocationsByGenerationAsync(pokemonName, generation,gameVersion, progress );
    }

    public Task<List<LocationArea>> GetLocationsByGenerationAsync(string pokemonName, string generation)
    {
        throw new NotImplementedException();
    }

    public Task<List<LocationArea>> GetLocationsByGameAsync(string pokemonName, string gameName)
    {
        throw new NotImplementedException();
    }
}
