
namespace PokeLens.Services.mappers;

public class GameGenerationMapper
{
    public static readonly Dictionary<string, string> GameToGeneration = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Geração I
            ["red"] = "generation-i",
            ["blue"] = "generation-i",
            ["yellow"] = "generation-i",
            ["green"] = "generation-i",
            
            // Geração II
            ["gold"] = "generation-ii",
            ["silver"] = "generation-ii",
            ["crystal"] = "generation-ii",
            
            // Geração III
            ["ruby"] = "generation-iii",
            ["sapphire"] = "generation-iii",
            ["emerald"] = "generation-iii",
            ["firered"] = "generation-iii",
            ["leafgreen"] = "generation-iii",
            ["colosseum"] = "generation-iii",
            ["xd"] = "generation-iii",
            
            // Geração IV
            ["diamond"] = "generation-iv",
            ["pearl"] = "generation-iv",
            ["platinum"] = "generation-iv",
            ["heartgold"] = "generation-iv",
            ["soulsilver"] = "generation-iv",
            
            // Geração V
            ["black"] = "generation-v",
            ["white"] = "generation-v",
            ["black-2"] = "generation-v",
            ["white-2"] = "generation-v",
            
            // Geração VI
            ["x"] = "generation-vi",
            ["y"] = "generation-vi",
            ["omega-ruby"] = "generation-vi",
            ["alpha-sapphire"] = "generation-vi",
            
            // Geração VII
            ["sun"] = "generation-vii",
            ["moon"] = "generation-vii",
            ["ultra-sun"] = "generation-vii",
            ["ultra-moon"] = "generation-vii",
            ["lets-go-pikachu"] = "generation-vii",
            ["lets-go-eevee"] = "generation-vii",
            
            // Geração VIII
            ["sword"] = "generation-viii",
            ["shield"] = "generation-viii",
            ["brilliant-diamond"] = "generation-viii",
            ["shining-pearl"] = "generation-viii",
            ["legends-arceus"] = "generation-viii",
            
            // Geração IX
            ["scarlet"] = "generation-ix",
            ["violet"] = "generation-ix",
            ["the-teal-mask"] = "generation-ix",
            ["the-indigo-disk"] = "generation-ix"
        };
    
    public static string GetGenerationByGame(string gameName)
    {
        if (GameToGeneration.TryGetValue(gameName.ToLower(), out var generation))
        {
            return generation;
        }
            
        throw new KeyNotFoundException($"Jogo '{gameName}' não reconhecido");
    }

    public static List<string> GetGamesByGeneration(string generation)
    {
        return GameToGeneration
            .Where(x => x.Value.Equals(generation, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Key)
            .ToList();
    }

    public static bool IsGameValid(string gameName)
    {
        return GameToGeneration.ContainsKey(gameName.ToLower());
    }

    public static string[] GetAllAvailableGames()
    {
        return GameToGeneration.Keys.ToArray();
    }
}
