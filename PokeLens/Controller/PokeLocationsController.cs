using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using PokeLens.Models;
using PokeLens.Models.Enums;
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
public async Task<ActionResult<PokemonLocationResponse>> GetPokemonLocationsGenerationIV(
    [FromRoute] string pokemonName,
    [FromQuery] string version = "HeartGold",
    [FromQuery] string? progressDisplayName = null)
{
    try
    {
        GameVersion gameVersion = Enum.Parse<GameVersion>(version, true);
        bool isHeartGold = gameVersion == GameVersion.HeartGold;
        var gameName = isHeartGold ? "heartgold" : "soulsilver";
        var generation = "generation-iv";
        var isExclusive = GameGenerationMapper.IsHeartGoldSoulSilverExclusive(pokemonName);
        var availableIn = GetAvailabilityText(pokemonName, isHeartGold);
       
        if (GameGenerationMapper.IsHeartGoldSoulSilverExclusive(pokemonName) &&
            ((GameGenerationMapper.IsHeartGold(pokemonName) && !isHeartGold) ||
             (GameGenerationMapper.IsSoulSilver(pokemonName) && isHeartGold)))
        {
            return NotFound(new
            {
                Error = $"O Pokémon {pokemonName} é exclusivo de {(GameGenerationMapper.IsHeartGold(pokemonName) ? "HeartGold" : "SoulSilver")} e não possui locais neste jogo.",
                Pokemon = pokemonName,
                Generation = generation
            });
        }

        // Converte o displayName da rota para o número do progresso
        int progress = int.MaxValue; 
        if (!string.IsNullOrEmpty(progressDisplayName))
        {
            var progressId = HeartGoldSoulSilverMapper.Locations
                .FirstOrDefault(l => l.Value.DisplayName == progressDisplayName).Key;
            if (progressId != 0)
            {
                progress = progressId;
            }
            else
            {
                return BadRequest(new
                {
                    Error = $"Rota '{progressDisplayName}' inválida.",
                    AvailableRoutes = HeartGoldSoulSilverMapper.Locations.Values
                        .Select(l => l.DisplayName)
                        .ToList()
                });
            }
        }

        var locations = await _pokeLocationService.GetLocationsByGenerationAsync(pokemonName, generation, gameName, progress);
        

        return Ok(new PokemonLocationResponse
        {
            Pokemon = pokemonName,
            Game = gameName,
            Generation = "IV",
            IsExclusive = isExclusive,
            Availability = availableIn,
            Locations = locations
        });
    }
    catch (Exception ex)
    {
        return NotFound(new
        {
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
            var locations = await _pokeLocationService.GetLocationsByGameAsync(pokemonName, gameName, gameName, int.MaxValue);
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