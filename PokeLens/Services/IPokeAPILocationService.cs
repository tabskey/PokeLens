using PokeLens.Models;


namespace PokeLens.Services;

public interface IPokeAPILocationService
{
    Task<List<LocationArea>> GetLocationsByGenerationAsync(string pokemonName, string generation);
    Task<List<LocationArea>> GetLocationsByGameAsync(string pokemonName, string gameName);
}