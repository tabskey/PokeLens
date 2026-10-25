namespace PokeBattle.Core.Damage;

/// <summary>How critical hits (2x in Generation III) are taken into account.</summary>
public enum CriticalHitPolicy
{
    /// <summary>Critical hits are ignored.</summary>
    Never,

    /// <summary>
    /// A critical hit is possible: the range goes from the lowest non-critical roll to the highest
    /// critical roll.
    /// </summary>
    Normal,

    /// <summary>Every hit is critical.</summary>
    Guaranteed,

    /// <summary>
    /// Non-critical and critical results are reported separately, together with the critical hit
    /// chance (1/16, or 1/8 for high critical ratio moves); the average is weighted by that chance.
    /// </summary>
    EvaluateProbability,
}