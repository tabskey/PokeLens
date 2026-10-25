namespace PokeBattle.Core.Damage;

/// <summary>
/// How the damage roll (the integer random factor 85..100) is evaluated. This is not part of the
/// battle scenario: the scenario describes assumed conditions, the policy describes the variance.
/// </summary>
public enum DamageRollPolicy
{
    /// <summary>Lowest possible damage (factor 85).</summary>
    Minimum,

    /// <summary>Mean of the 16 integer results, respecting the real roll distribution.</summary>
    Average,

    /// <summary>Highest possible damage (factor 100).</summary>
    Maximum,

    /// <summary>Min, max and every one of the 16 integer results.</summary>
    FullRange,
}