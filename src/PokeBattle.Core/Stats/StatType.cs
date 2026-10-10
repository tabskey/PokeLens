namespace PokeBattle.Core.Stats;

/// <summary>Stats affected by Nature. HP is intentionally excluded: Nature never changes it.</summary>
public enum StatType
{
    Attack,
    Defense,
    SpecialAttack,
    SpecialDefense,
    Speed,
}