using System.Text.Json.Serialization;

namespace PokeLens.Models;

public class PokeLocation
{
    [JsonPropertyName("location_area")]
    public LocationArea LocationArea { get; set; } = new();
    
    [JsonPropertyName("version_details")]
    public List<VersionDetail> VersionDetails { get; set; } = new();
}
public class LocationArea
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public class VersionDetail
{
    [JsonPropertyName("version")]
    public PokemonVersion Version { get; set; } = new();
    
    [JsonPropertyName("max_chance")]
    public int MaxChance { get; set; }
    
    [JsonPropertyName("encounter_details")]
    public List<EncounterDetail> EncounterDetails { get; set; } = new();
}

public class PokemonVersion
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}

public class EncounterDetail
{
    [JsonPropertyName("method")]
    public Method Method { get; set; } = new();
    
    [JsonPropertyName("chance")]
    public int Chance { get; set; }
}

public class Method
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
public class PokemonLocationResponse
{
    public string Pokemon { get; set; } = string.Empty;
    public string Game { get; set; } = string.Empty;
    public string Generation { get; set; } = string.Empty;
    public bool IsExclusive { get; set; }
    public string Availability { get; set; } = string.Empty;
    public List<LocationArea> Locations { get; set; } = new();
}