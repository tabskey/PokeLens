using System.Text.Json;
using PokeLens.Models;

namespace PokeLens.Services;

public class PokeApiService : IPokeApiService
{
    private readonly HttpClient _httpClient;

    public PokeApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PokeSpecies> GetPokemonSpeciesAsync(string pokemonName)
    {
        try
        {
            var response = await _httpClient.GetAsync($"pokemon/{pokemonName.ToLower()}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonSerializer.Deserialize<PokeSpecies>(content, options);

            if (result == null) throw new Exception("Failed to deserialize Pokemon data");

            return result;
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Error fetching Pokemon: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new Exception($"Error processing Pokemon data: {ex.Message}", ex);
        }
    }
}