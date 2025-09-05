using System.Text.Json;
using PokeLens.Models;
using System.Text.Json.Serialization;
using PokeLens.Services;

namespace PokeLens.Services;
public class PokeApiService : IPokeApiService
{
    private readonly HttpClient _httpClient;

    public PokeApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://pokeapi.co/api/v2/");
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
                PropertyNameCaseInsensitive = true,
            };
            
            var result = JsonSerializer.Deserialize<PokeSpecies>(content, options);
            
            if (result == null)
            {
                throw new Exception("Falha ao desserializar os dados do Pokémon");
            }
            
            return result;
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Erro ao buscar Pokémon: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new Exception($"Erro ao processar dados do Pokémon: {ex.Message}", ex);
        }
    }
}