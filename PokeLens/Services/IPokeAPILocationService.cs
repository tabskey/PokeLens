using PokeLens.Models;


namespace PokeLens.Services;

public interface IPokeAPILocationService
{
    Task<List<LocationArea>> GetLocationsByGenerationAsync(string pokemonName, string generation, string gameVersion,  int progress);
    Task<List<LocationArea>> GetLocationsByGameAsync(string pokemonName, string gameName,  string gameVersion, int progress);
}