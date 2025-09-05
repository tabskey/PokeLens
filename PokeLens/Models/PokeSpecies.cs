using System.Text.Json.Serialization;

namespace PokeLens.Models;

public class PokeSpecies
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("abilities")]
    public List<PokemonAbility> Abilities { get; set; } = new();
   
}

   public class PokemonAbility
{
    [JsonPropertyName("ability")]
    public Ability Ability { get; set; } = new();
    [JsonPropertyName("is_hidden")]
    public bool IsHidden { get; set; }
    [JsonPropertyName("slot")]
    public int Slot { get; set; }
}
   public class Ability
   {
       [JsonPropertyName("name")]
       public string Name { get; set; } = string.Empty;
       [JsonPropertyName("url")]
       public string Url { get; set; } = string.Empty;
   }
