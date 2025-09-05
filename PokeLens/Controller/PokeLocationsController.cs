using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using PokeLens.Models;
using PokeLens.Services;
using PokeLens.Services.mappers;

namespace PokeLens.Controller;

    [ApiController]
    [Route("api/pokemon/{pokemonName}/[controller]")]
    public class PokeLocationController : ControllerBase
    {
        private readonly IPokeAPILocationService _pokeLocationService;
        private readonly ILogger<PokeLocationController> _logger;

        public PokeLocationController(
            IPokeAPILocationService locationService,
            ILogger<PokeLocationController> logger)
        {
            _pokeLocationService = locationService;
            _logger = logger;
        }

        
    [HttpGet("generation-ii")]
    public async Task<ActionResult> GetPokemonLocationsGenerationIi(string pokemonName)
    {
        try
        {
            var locations = await _pokeLocationService.GetLocationsByGenerationAsync(pokemonName, "generation-ii");
            return Ok(new {
                Pokemon = pokemonName,
                Generation = "generation-ii",
                Games = new[] { "gold", "silver", "crystal" },
                Locations = locations
            });
        }
        catch (Exception ex)
        {
            return NotFound(new {
                Error = ex.Message,
                Pokemon = pokemonName,
                Generation = "generation-ii"
            });
        }
    }
    
    [HttpGet("game/{gameName}")]
    public async Task<ActionResult> GetPokemonLocationsByGame(string pokemonName, string gameName)
    {
        try
        {
            var generation = GameGenerationMapper.GetGenerationByGame(gameName);
            var locations = await _pokeLocationService.GetLocationsByGameAsync(pokemonName, gameName);
            var allGamesInGeneration = GameGenerationMapper.GetGamesByGeneration(generation);
                
            return Ok(new {
                Pokemon = pokemonName,
                Game = gameName,
                Generation = generation,
                AllGamesInGeneration = allGamesInGeneration,
                Locations = locations
            });
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new {
                Error = ex.Message,
                Game = gameName,
                AvailableGames = GameGenerationMapper.GetAllAvailableGames()
            });
        }
        catch (Exception ex)
        {
            return NotFound(new {
                Error = ex.Message,
                Pokemon = pokemonName,
                Game = gameName
            });
        }
    }
}