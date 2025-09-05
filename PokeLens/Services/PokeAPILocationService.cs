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
    public async Task<List<LocationArea>> GetLocationsByGenerationAsync(string pokemonName, string generation)
    {
        try
        {
            _logger.LogInformation($"Buscando localizações do {pokemonName} na geração {generation}");
            
            // Primeiro, verifica se o Pokémon existe
            var pokemonResponse = await _httpClient.GetAsync($"pokemon/{pokemonName.ToLower()}");
            if (!pokemonResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Pokémon {pokemonName} não encontrado");
            }

            // Busca as localizações do Pokémon
            var locationsResponse = await _httpClient.GetAsync($"pokemon/{pokemonName.ToLower()}/encounters");
            locationsResponse.EnsureSuccessStatusCode();
            
            var locationsContent = await locationsResponse.Content.ReadAsStringAsync();
            var allLocations = JsonSerializer.Deserialize<List<PokeLocation>>(locationsContent, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<PokeLocation>();

            // Filtra as localizações pela geração
            var versionsInGeneration = GameGenerationMapper.GetGamesByGeneration(generation);
            var filteredLocations = new List<LocationArea>(); 

            foreach (var location in allLocations)
            {
                // Verifica se há encontros nesta localização para a geração específica
                var hasGenerationEncounters = location.VersionDetails
                    .Any(vd => versionsInGeneration.Contains(vd.Version.Name));

                if (hasGenerationEncounters)
                {
                    filteredLocations.Add(location.LocationArea);
                }
            }

            if (!filteredLocations.Any())
            {
                throw new Exception($"O Pokémon {pokemonName} não pode ser encontrado na geração {generation}");
            }

            return filteredLocations;
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
    public async Task<List<LocationArea>> GetLocationsByGameAsync(string pokemonName, string gameName)
    {
        var generation = GameGenerationMapper.GetGenerationByGame(gameName);
        return await GetLocationsByGenerationAsync(pokemonName, generation);
    }
    
}
