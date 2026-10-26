namespace PokeBattle.Core.Types;

/// <summary>
/// Generation III type effectiveness chart (Steel still resists Ghost and Dark, which Gen VI later
/// removed). Any attacker/defender pair not listed is neutral (1x).
/// </summary>
/// <remarks>
/// The entries mirror <c>gTypeEffectiveness</c> from the FireRed decompilation (pret/pokefirered,
/// src/battle_main.c) in the same order. Order matters: the game applies each matching entry to the
/// damage one at a time, truncating after each step, so a 2x and a 0.5x on a dual-type defender do
/// not always cancel out exactly. Multipliers are kept in tenths, as in the game.
/// </remarks>
public static class TypeEffectivenessService
{
    private const int SuperEffective = 20;
    private const int NotVeryEffective = 5;
    private const int NoEffect = 0;

    private static readonly Entry[] Chart =
    [
        new(PokemonType.Normal, PokemonType.Rock, NotVeryEffective),
        new(PokemonType.Normal, PokemonType.Steel, NotVeryEffective),

        new(PokemonType.Fire, PokemonType.Fire, NotVeryEffective),
        new(PokemonType.Fire, PokemonType.Water, NotVeryEffective),
        new(PokemonType.Fire, PokemonType.Grass, SuperEffective),
        new(PokemonType.Fire, PokemonType.Ice, SuperEffective),
        new(PokemonType.Fire, PokemonType.Bug, SuperEffective),
        new(PokemonType.Fire, PokemonType.Rock, NotVeryEffective),
        new(PokemonType.Fire, PokemonType.Dragon, NotVeryEffective),
        new(PokemonType.Fire, PokemonType.Steel, SuperEffective),

        new(PokemonType.Water, PokemonType.Fire, SuperEffective),
        new(PokemonType.Water, PokemonType.Water, NotVeryEffective),
        new(PokemonType.Water, PokemonType.Grass, NotVeryEffective),
        new(PokemonType.Water, PokemonType.Ground, SuperEffective),
        new(PokemonType.Water, PokemonType.Rock, SuperEffective),
        new(PokemonType.Water, PokemonType.Dragon, NotVeryEffective),

        new(PokemonType.Electric, PokemonType.Water, SuperEffective),
        new(PokemonType.Electric, PokemonType.Electric, NotVeryEffective),
        new(PokemonType.Electric, PokemonType.Grass, NotVeryEffective),
        new(PokemonType.Electric, PokemonType.Ground, NoEffect),
        new(PokemonType.Electric, PokemonType.Flying, SuperEffective),
        new(PokemonType.Electric, PokemonType.Dragon, NotVeryEffective),

        new(PokemonType.Grass, PokemonType.Fire, NotVeryEffective),
        new(PokemonType.Grass, PokemonType.Water, SuperEffective),
        new(PokemonType.Grass, PokemonType.Grass, NotVeryEffective),
        new(PokemonType.Grass, PokemonType.Poison, NotVeryEffective),
        new(PokemonType.Grass, PokemonType.Ground, SuperEffective),
        new(PokemonType.Grass, PokemonType.Flying, NotVeryEffective),
        new(PokemonType.Grass, PokemonType.Bug, NotVeryEffective),
        new(PokemonType.Grass, PokemonType.Rock, SuperEffective),
        new(PokemonType.Grass, PokemonType.Dragon, NotVeryEffective),
        new(PokemonType.Grass, PokemonType.Steel, NotVeryEffective),

        new(PokemonType.Ice, PokemonType.Water, NotVeryEffective),
        new(PokemonType.Ice, PokemonType.Grass, SuperEffective),
        new(PokemonType.Ice, PokemonType.Ice, NotVeryEffective),
        new(PokemonType.Ice, PokemonType.Ground, SuperEffective),
        new(PokemonType.Ice, PokemonType.Flying, SuperEffective),
        new(PokemonType.Ice, PokemonType.Dragon, SuperEffective),
        new(PokemonType.Ice, PokemonType.Steel, NotVeryEffective),
        new(PokemonType.Ice, PokemonType.Fire, NotVeryEffective),

        new(PokemonType.Fighting, PokemonType.Normal, SuperEffective),
        new(PokemonType.Fighting, PokemonType.Ice, SuperEffective),
        new(PokemonType.Fighting, PokemonType.Poison, NotVeryEffective),
        new(PokemonType.Fighting, PokemonType.Flying, NotVeryEffective),
        new(PokemonType.Fighting, PokemonType.Psychic, NotVeryEffective),
        new(PokemonType.Fighting, PokemonType.Bug, NotVeryEffective),
        new(PokemonType.Fighting, PokemonType.Rock, SuperEffective),
        new(PokemonType.Fighting, PokemonType.Dark, SuperEffective),
        new(PokemonType.Fighting, PokemonType.Steel, SuperEffective),

        new(PokemonType.Poison, PokemonType.Grass, SuperEffective),
        new(PokemonType.Poison, PokemonType.Poison, NotVeryEffective),
        new(PokemonType.Poison, PokemonType.Ground, NotVeryEffective),
        new(PokemonType.Poison, PokemonType.Rock, NotVeryEffective),
        new(PokemonType.Poison, PokemonType.Ghost, NotVeryEffective),
        new(PokemonType.Poison, PokemonType.Steel, NoEffect),

        new(PokemonType.Ground, PokemonType.Fire, SuperEffective),
        new(PokemonType.Ground, PokemonType.Electric, SuperEffective),
        new(PokemonType.Ground, PokemonType.Grass, NotVeryEffective),
        new(PokemonType.Ground, PokemonType.Poison, SuperEffective),
        new(PokemonType.Ground, PokemonType.Flying, NoEffect),
        new(PokemonType.Ground, PokemonType.Bug, NotVeryEffective),
        new(PokemonType.Ground, PokemonType.Rock, SuperEffective),
        new(PokemonType.Ground, PokemonType.Steel, SuperEffective),

        new(PokemonType.Flying, PokemonType.Electric, NotVeryEffective),
        new(PokemonType.Flying, PokemonType.Grass, SuperEffective),
        new(PokemonType.Flying, PokemonType.Fighting, SuperEffective),
        new(PokemonType.Flying, PokemonType.Bug, SuperEffective),
        new(PokemonType.Flying, PokemonType.Rock, NotVeryEffective),
        new(PokemonType.Flying, PokemonType.Steel, NotVeryEffective),

        new(PokemonType.Psychic, PokemonType.Fighting, SuperEffective),
        new(PokemonType.Psychic, PokemonType.Poison, SuperEffective),
        new(PokemonType.Psychic, PokemonType.Psychic, NotVeryEffective),
        new(PokemonType.Psychic, PokemonType.Dark, NoEffect),
        new(PokemonType.Psychic, PokemonType.Steel, NotVeryEffective),

        new(PokemonType.Bug, PokemonType.Fire, NotVeryEffective),
        new(PokemonType.Bug, PokemonType.Grass, SuperEffective),
        new(PokemonType.Bug, PokemonType.Fighting, NotVeryEffective),
        new(PokemonType.Bug, PokemonType.Poison, NotVeryEffective),
        new(PokemonType.Bug, PokemonType.Flying, NotVeryEffective),
        new(PokemonType.Bug, PokemonType.Psychic, SuperEffective),
        new(PokemonType.Bug, PokemonType.Ghost, NotVeryEffective),
        new(PokemonType.Bug, PokemonType.Dark, SuperEffective),
        new(PokemonType.Bug, PokemonType.Steel, NotVeryEffective),

        new(PokemonType.Rock, PokemonType.Fire, SuperEffective),
        new(PokemonType.Rock, PokemonType.Ice, SuperEffective),
        new(PokemonType.Rock, PokemonType.Fighting, NotVeryEffective),
        new(PokemonType.Rock, PokemonType.Ground, NotVeryEffective),
        new(PokemonType.Rock, PokemonType.Flying, SuperEffective),
        new(PokemonType.Rock, PokemonType.Bug, SuperEffective),
        new(PokemonType.Rock, PokemonType.Steel, NotVeryEffective),

        new(PokemonType.Ghost, PokemonType.Normal, NoEffect),
        new(PokemonType.Ghost, PokemonType.Psychic, SuperEffective),
        new(PokemonType.Ghost, PokemonType.Dark, NotVeryEffective),
        new(PokemonType.Ghost, PokemonType.Steel, NotVeryEffective),
        new(PokemonType.Ghost, PokemonType.Ghost, SuperEffective),

        new(PokemonType.Dragon, PokemonType.Dragon, SuperEffective),
        new(PokemonType.Dragon, PokemonType.Steel, NotVeryEffective),

        new(PokemonType.Dark, PokemonType.Fighting, NotVeryEffective),
        new(PokemonType.Dark, PokemonType.Psychic, SuperEffective),
        new(PokemonType.Dark, PokemonType.Ghost, SuperEffective),
        new(PokemonType.Dark, PokemonType.Dark, NotVeryEffective),
        new(PokemonType.Dark, PokemonType.Steel, NotVeryEffective),

        new(PokemonType.Steel, PokemonType.Fire, NotVeryEffective),
        new(PokemonType.Steel, PokemonType.Water, NotVeryEffective),
        new(PokemonType.Steel, PokemonType.Electric, NotVeryEffective),
        new(PokemonType.Steel, PokemonType.Ice, SuperEffective),
        new(PokemonType.Steel, PokemonType.Rock, SuperEffective),
        new(PokemonType.Steel, PokemonType.Steel, NotVeryEffective),

        // Foresight marker in the original table: the entries below are skipped when the target is
        // identified by Foresight. Foresight is out of scope, so they always apply (still last, in order).
        new(PokemonType.Normal, PokemonType.Ghost, NoEffect),

        new(PokemonType.Fighting, PokemonType.Ghost, NoEffect),
    ];

    public static double GetMultiplier(PokemonType attackingType, PokemonType defenderType)
    {
        foreach (var entry in Chart)
        {
            if (entry.Attacker == attackingType && entry.Defender == defenderType)
            {
                return entry.Tenths / 10.0;
            }
        }

        return 1.0;
    }

    public static double GetMultiplier(PokemonType attackingType, PokemonType defenderType1, PokemonType? defenderType2)
    {
        var multiplier = GetMultiplier(attackingType, defenderType1);
        return defenderType2.HasValue ? multiplier * GetMultiplier(attackingType, defenderType2.Value) : multiplier;
    }

    /// <summary>
    /// Applies type effectiveness to an integer damage value exactly like the game's typecalc: walks
    /// the chart in order and, for every entry matching one of the defender's types, does
    /// <c>damage * tenths / 10</c>. A non-zero multiplier never drops damage below 1.
    /// </summary>
    public static int ApplyTo(int damage, PokemonType moveType, PokemonType defenderType1, PokemonType? defenderType2)
    {
        foreach (var entry in Chart)
        {
            if (entry.Attacker != moveType)
            {
                continue;
            }

            if (entry.Defender == defenderType1)
            {
                damage = Modulate(damage, entry.Tenths);
            }

            if (entry.Defender == defenderType2 && defenderType1 != defenderType2)
            {
                damage = Modulate(damage, entry.Tenths);
            }
        }

        return damage;
    }

    private static int Modulate(int damage, int tenths)
    {
        var modulated = damage * tenths / 10;
        return modulated == 0 && tenths != NoEffect ? 1 : modulated;
    }

    private readonly record struct Entry(PokemonType Attacker, PokemonType Defender, int Tenths);
}