namespace PokeBattle.Core.Battles;

/// <summary>Remaining HP after one hit, from the strongest roll (Min) to the weakest roll (Max).</summary>
public sealed record HpRange(int Min, int Max)
{
    public static HpRange AfterHit(int targetHp, int minDamage, int maxDamage)
        => new(Math.Max(0, targetHp - maxDamage), Math.Max(0, targetHp - minDamage));
}