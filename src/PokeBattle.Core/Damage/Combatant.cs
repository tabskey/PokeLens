using PokeBattle.Core.Types;

namespace PokeBattle.Core.Damage;

/// <summary>
/// A Pokémon as seen by the damage formula: level, final stats (already computed by
/// <see cref="Stats.StatCalculator"/>) and types.
/// </summary>
public sealed record Combatant(
    int Level,
    int Attack,
    int Defense,
    int SpecialAttack,
    int SpecialDefense,
    PokemonType Type1,
    PokemonType? Type2 = null)
{
    public bool HasType(PokemonType type) => Type1 == type || Type2 == type;
}