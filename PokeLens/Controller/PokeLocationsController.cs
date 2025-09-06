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

        
    [HttpGet("generation-iv")]
    public async Task<ActionResult> GetPokemonLocationsGenerationIV(string pokemonName,
                                                                  [FromQuery] bool isHeartGold = true)
    {
        try
        {
            var gameName = isHeartGold ? "heartgold" : "soulsilver";
            var locations = await _pokeLocationService.GetLocationsByGenerationAsync(pokemonName, 
                                                                                            "generation-iv");
            
            var isExclusive = GameGenerationMapper.IsHeartGoldSoulSilverExclusive(pokemonName);
            var availableIn = GetAvailabilityText(pokemonName, isHeartGold);
            var exclusiveTo = isExclusive ? 
                (isHeartGold ? "HeartGold" : "SoulSilver") : 
                "Both";
            
            return Ok(new {
                Pokemon = pokemonName,
                Game = isHeartGold ? "HeartGold" : "SoulSilver",
                Generation = "IV",
                IsExclusive = isExclusive,
                Availability = availableIn,
                Locations = locations
            });
        }
        catch (Exception ex)
        {
            return NotFound(new {
                Error = ex.Message,
                Pokemon = pokemonName,
                Generation = "generation-iv"
            });
        }
        
    }
    private string GetAvailabilityText(string pokemonName, bool isHeartGold)
    {
        var availability = GameGenerationMapper.GetHeartGoldSoulSilverAvailability(pokemonName);
        return availability switch
        {
            "HeartGold Exclusive" => isHeartGold ? 
                "Available (HeartGold Exclusive)" : "Unavailable (HeartGold Exclusive)",
        
            "SoulSilver Exclusive" => isHeartGold ? 
                "Unavailable (SoulSilver Exclusive)" : "Available (SoulSilver Exclusive)",
        
            _ => "Available in both games"
        };
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