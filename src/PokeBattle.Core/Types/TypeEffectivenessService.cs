namespace PokeBattle.Core.Types;

/// <summary>
/// Generation III type effectiveness chart (identical to Gen I/II/IV/V: Steel still resists Ghost
/// and Dark, which Gen VI later removed). Any attacker/defender pair not listed is neutral (1x).
/// </summary>
public static class TypeEffectivenessService
{
    private static readonly Dictionary<(PokemonType Attacker, PokemonType Defender), double> Chart = new()
    {
        [(PokemonType.Normal, PokemonType.Rock)] = 0.5,
        [(PokemonType.Normal, PokemonType.Ghost)] = 0,
        [(PokemonType.Normal, PokemonType.Steel)] = 0.5,

        [(PokemonType.Fighting, PokemonType.Normal)] = 2,
        [(PokemonType.Fighting, PokemonType.Flying)] = 0.5,
        [(PokemonType.Fighting, PokemonType.Poison)] = 0.5,
        [(PokemonType.Fighting, PokemonType.Rock)] = 2,
        [(PokemonType.Fighting, PokemonType.Bug)] = 0.5,
        [(PokemonType.Fighting, PokemonType.Ghost)] = 0,
        [(PokemonType.Fighting, PokemonType.Steel)] = 2,
        [(PokemonType.Fighting, PokemonType.Psychic)] = 0.5,
        [(PokemonType.Fighting, PokemonType.Ice)] = 2,
        [(PokemonType.Fighting, PokemonType.Dark)] = 2,

        [(PokemonType.Flying, PokemonType.Fighting)] = 2,
        [(PokemonType.Flying, PokemonType.Rock)] = 0.5,
        [(PokemonType.Flying, PokemonType.Bug)] = 2,
        [(PokemonType.Flying, PokemonType.Steel)] = 0.5,
        [(PokemonType.Flying, PokemonType.Grass)] = 2,
        [(PokemonType.Flying, PokemonType.Electric)] = 0.5,

        [(PokemonType.Poison, PokemonType.Grass)] = 2,
        [(PokemonType.Poison, PokemonType.Poison)] = 0.5,
        [(PokemonType.Poison, PokemonType.Ground)] = 0.5,
        [(PokemonType.Poison, PokemonType.Rock)] = 0.5,
        [(PokemonType.Poison, PokemonType.Ghost)] = 0.5,
        [(PokemonType.Poison, PokemonType.Steel)] = 0,

        [(PokemonType.Ground, PokemonType.Fire)] = 2,
        [(PokemonType.Ground, PokemonType.Electric)] = 2,
        [(PokemonType.Ground, PokemonType.Grass)] = 0.5,
        [(PokemonType.Ground, PokemonType.Poison)] = 2,
        [(PokemonType.Ground, PokemonType.Flying)] = 0,
        [(PokemonType.Ground, PokemonType.Bug)] = 0.5,
        [(PokemonType.Ground, PokemonType.Rock)] = 2,
        [(PokemonType.Ground, PokemonType.Steel)] = 2,

        [(PokemonType.Rock, PokemonType.Fire)] = 2,
        [(PokemonType.Rock, PokemonType.Ice)] = 2,
        [(PokemonType.Rock, PokemonType.Flying)] = 2,
        [(PokemonType.Rock, PokemonType.Bug)] = 2,
        [(PokemonType.Rock, PokemonType.Fighting)] = 0.5,
        [(PokemonType.Rock, PokemonType.Ground)] = 0.5,
        [(PokemonType.Rock, PokemonType.Steel)] = 0.5,

        [(PokemonType.Bug, PokemonType.Grass)] = 2,
        [(PokemonType.Bug, PokemonType.Fire)] = 0.5,
        [(PokemonType.Bug, PokemonType.Fighting)] = 0.5,
        [(PokemonType.Bug, PokemonType.Flying)] = 0.5,
        [(PokemonType.Bug, PokemonType.Poison)] = 0.5,
        [(PokemonType.Bug, PokemonType.Ghost)] = 0.5,
        [(PokemonType.Bug, PokemonType.Steel)] = 0.5,
        [(PokemonType.Bug, PokemonType.Psychic)] = 2,
        [(PokemonType.Bug, PokemonType.Dark)] = 2,

        [(PokemonType.Ghost, PokemonType.Ghost)] = 2,
        [(PokemonType.Ghost, PokemonType.Normal)] = 0,
        [(PokemonType.Ghost, PokemonType.Dark)] = 0.5,
        [(PokemonType.Ghost, PokemonType.Steel)] = 0.5,

        [(PokemonType.Steel, PokemonType.Steel)] = 0.5,
        [(PokemonType.Steel, PokemonType.Fire)] = 0.5,
        [(PokemonType.Steel, PokemonType.Water)] = 0.5,
        [(PokemonType.Steel, PokemonType.Electric)] = 0.5,
        [(PokemonType.Steel, PokemonType.Ice)] = 2,
        [(PokemonType.Steel, PokemonType.Rock)] = 2,

        [(PokemonType.Fire, PokemonType.Fire)] = 0.5,
        [(PokemonType.Fire, PokemonType.Water)] = 0.5,
        [(PokemonType.Fire, PokemonType.Grass)] = 2,
        [(PokemonType.Fire, PokemonType.Ice)] = 2,
        [(PokemonType.Fire, PokemonType.Bug)] = 2,
        [(PokemonType.Fire, PokemonType.Rock)] = 0.5,
        [(PokemonType.Fire, PokemonType.Dragon)] = 0.5,
        [(PokemonType.Fire, PokemonType.Steel)] = 2,

        [(PokemonType.Water, PokemonType.Fire)] = 2,
        [(PokemonType.Water, PokemonType.Water)] = 0.5,
        [(PokemonType.Water, PokemonType.Grass)] = 0.5,
        [(PokemonType.Water, PokemonType.Ground)] = 2,
        [(PokemonType.Water, PokemonType.Rock)] = 2,
        [(PokemonType.Water, PokemonType.Dragon)] = 0.5,

        [(PokemonType.Grass, PokemonType.Fire)] = 0.5,
        [(PokemonType.Grass, PokemonType.Water)] = 2,
        [(PokemonType.Grass, PokemonType.Grass)] = 0.5,
        [(PokemonType.Grass, PokemonType.Poison)] = 0.5,
        [(PokemonType.Grass, PokemonType.Ground)] = 2,
        [(PokemonType.Grass, PokemonType.Flying)] = 0.5,
        [(PokemonType.Grass, PokemonType.Bug)] = 0.5,
        [(PokemonType.Grass, PokemonType.Rock)] = 2,
        [(PokemonType.Grass, PokemonType.Dragon)] = 0.5,
        [(PokemonType.Grass, PokemonType.Steel)] = 0.5,

        [(PokemonType.Electric, PokemonType.Water)] = 2,
        [(PokemonType.Electric, PokemonType.Electric)] = 0.5,
        [(PokemonType.Electric, PokemonType.Grass)] = 0.5,
        [(PokemonType.Electric, PokemonType.Ground)] = 0,
        [(PokemonType.Electric, PokemonType.Flying)] = 2,
        [(PokemonType.Electric, PokemonType.Dragon)] = 0.5,

        [(PokemonType.Psychic, PokemonType.Fighting)] = 2,
        [(PokemonType.Psychic, PokemonType.Poison)] = 2,
        [(PokemonType.Psychic, PokemonType.Psychic)] = 0.5,
        [(PokemonType.Psychic, PokemonType.Steel)] = 0.5,
        [(PokemonType.Psychic, PokemonType.Dark)] = 0,

        [(PokemonType.Ice, PokemonType.Fire)] = 0.5,
        [(PokemonType.Ice, PokemonType.Water)] = 0.5,
        [(PokemonType.Ice, PokemonType.Ice)] = 0.5,
        [(PokemonType.Ice, PokemonType.Ground)] = 2,
        [(PokemonType.Ice, PokemonType.Flying)] = 2,
        [(PokemonType.Ice, PokemonType.Dragon)] = 2,
        [(PokemonType.Ice, PokemonType.Steel)] = 0.5,

        [(PokemonType.Dragon, PokemonType.Dragon)] = 2,
        [(PokemonType.Dragon, PokemonType.Steel)] = 0.5,

        [(PokemonType.Dark, PokemonType.Fighting)] = 0.5,
        [(PokemonType.Dark, PokemonType.Ghost)] = 2,
        [(PokemonType.Dark, PokemonType.Psychic)] = 2,
        [(PokemonType.Dark, PokemonType.Dark)] = 0.5,
        [(PokemonType.Dark, PokemonType.Steel)] = 0.5,
    };

    public static double GetMultiplier(PokemonType attackingType, PokemonType defenderType)
        => Chart.TryGetValue((attackingType, defenderType), out var multiplier) ? multiplier : 1.0;

    public static double GetMultiplier(PokemonType attackingType, PokemonType defenderType1, PokemonType? defenderType2)
    {
        var multiplier = GetMultiplier(attackingType, defenderType1);
        return defenderType2.HasValue ? multiplier * GetMultiplier(attackingType, defenderType2.Value) : multiplier;
    }
}