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
            _logger.LogError(ex, $"Error fetching species for Pokemon {char.ToUpper(pokemonName[0]) + pokemonName.Substring(1)}");
                
            if (ex.Message.Contains("Not Found") || ex.Message.Contains("404"))
            {
                return NotFound(new { message = $"Pokemon '{char.ToUpper(pokemonName[0]) + pokemonName.Substring(1)}' not found" });
            }
                
            return StatusCode(500, new { message = ex.Message });
        }
    }
    
}