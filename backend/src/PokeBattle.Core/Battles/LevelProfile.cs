namespace PokeBattle.Core.Battles;

/// <summary>Which level of a <see cref="LevelRange"/> a scenario assumes.</summary>
public enum LevelProfile
{
    Lowest,

    /// <summary>floor((Min + Max) / 2).</summary>
    Midpoint,

    Highest,
}