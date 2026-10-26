using PokeBattle.Core.Types;

namespace PokeBattle.Core.Damage;

/// <summary>
/// A damaging move with a fixed base power. Status moves and fixed or variable damage moves
/// (Seismic Toss, Dragon Rage, Low Kick, multi-hit...) are out of scope for the V1 calculator.
/// </summary>
public sealed record Move
{
    public Move(PokemonType type, int power, bool highCriticalRatio = false)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(power);

        Type = type;
        Power = power;
        HighCriticalRatio = highCriticalRatio;
    }

    public PokemonType Type { get; }

    public int Power { get; }

    /// <summary>Slash, Razor Leaf, Karate Chop, Crabhammer...: critical hit stage +1 (1/8 instead of 1/16).</summary>
    public bool HighCriticalRatio { get; }
}