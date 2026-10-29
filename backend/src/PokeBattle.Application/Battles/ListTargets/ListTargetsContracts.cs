namespace PokeBattle.Application.Battles.ListTargets;

/// <summary>A Pokémon the player can meet at the milestone and every way to meet it. Id is the National Pokédex number.</summary>
public sealed record TargetOption(string Pokemon, int Id, IReadOnlyList<TargetEncounter> Encounters);

/// <param name="Chance">Encounter rate of the species with that method in that area, summed over its slots.</param>
public sealed record TargetEncounter(string Area, string DisplayName, string Method, int MinLevel, int MaxLevel, int Chance);
