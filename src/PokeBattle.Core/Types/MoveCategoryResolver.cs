namespace PokeBattle.Core.Types;

/// <summary>
/// In Generation III, a move's category is determined by its type (a fixed table), never by the
/// modern PokeAPI "damage_class" metadata. For example Bite is Dark-type and therefore Special,
/// even though later generations classify it as Physical.
/// </summary>
public static class MoveCategoryResolver
{
    public static MoveCategory GetCategory(PokemonType moveType) => moveType switch
    {
        PokemonType.Normal or PokemonType.Fighting or PokemonType.Flying or PokemonType.Poison or
            PokemonType.Ground or PokemonType.Rock or PokemonType.Bug or PokemonType.Ghost or PokemonType.Steel
            => MoveCategory.Physical,
        PokemonType.Fire or PokemonType.Water or PokemonType.Grass or PokemonType.Electric or
            PokemonType.Psychic or PokemonType.Ice or PokemonType.Dragon or PokemonType.Dark
            => MoveCategory.Special,
        _ => throw new ArgumentOutOfRangeException(nameof(moveType), moveType, "Unknown Generation III type."),
    };
}