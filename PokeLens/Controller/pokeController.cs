using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using PokeLens.Models;
using PokeLens.Services;
using PokeLens.Services.mappers;

namespace PokeLens.Controller;

[ApiController ]
[Route("api/[controller]")]
public class PokeController : ControllerBase
{
    private readonly IPokeApiService _pokeApiService;
    private readonly PokeLocationService _pokeLocationService;
    private readonly ILogger<PokeController> _logger;
    
    public PokeController(IPokeApiService pokeApiService,
        PokeLocationService pokeLocationService,
        ILogger<PokeController> logger)
        {
        _pokeApiService = pokeApiService;
        _pokeLocationService = pokeLocationService;
        _logger = logger;
        }
    // GET: api/pokemon/species/aegislash
    [HttpGet("species/{pokemonName}")]
    public async Task<ActionResult<PokeSpecies>> GetPokemonSpecies(string pokemonName)
    {
        try
        {
            var species = await _pokeApiService.GetPokemonSpeciesAsync(pokemonName);
            return Ok(species);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao buscar espécie do Pokémon {pokemonName}");
                
            if (ex.Message.Contains("não encontrado") || ex.Message.Contains("404"))
            {
                return NotFound(new { message = $"Pokémon '{pokemonName}' não encontrado" });
            }
                
            return StatusCode(500, new { message = ex.Message });
        }
    }
    
}