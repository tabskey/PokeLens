using PokeLens.Models;

namespace PokeLens.Services

{
    public interface IPokeApiService
    {
        Task<PokeSpecies> GetPokemonSpeciesAsync(string pokemonName);
       // Task<PokeSpecies> GetPokemonSpeciesByIdAsync(int id);
    }
}