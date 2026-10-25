namespace PokeBattle.Core.Battles;

/// <summary>Label derived from <see cref="TurnsToKo"/>. It summarises the numbers and never replaces them.</summary>
public enum KoClassification
{
    /// <summary>The attack deals no damage.</summary>
    NoDamage,

    /// <summary>One hit is enough with every roll.</summary>
    GuaranteedKo,

    /// <summary>One hit is enough with the best rolls only.</summary>
    PossibleKo,

    /// <summary>Two hits in the best case.</summary>
    TwoHitKo,

    /// <summary>Three hits in the best case.</summary>
    ThreeHitKo,

    FourOrMoreHits,
}