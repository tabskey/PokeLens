using PokeBattle.Core.Damage;

namespace PokeBattle.Core.Battles;

/// <summary>A Pokémon with every attribute settled by a scenario, ready for the damage formula.</summary>
public sealed record ResolvedBattler(int Level, int MaxHp, Combatant Combatant);