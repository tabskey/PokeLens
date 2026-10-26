namespace PokeBattle.Core.Battles;

/// <summary>Inclusive level range, e.g. the levels a wild Pokémon can have on a route.</summary>
public sealed record LevelRange
{
    public LevelRange(int min, int max)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(min, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(max, 100);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(min, max);

        Min = min;
        Max = max;
    }

    public int Min { get; }

    public int Max { get; }

    public static LevelRange Exactly(int level) => new(level, level);
}