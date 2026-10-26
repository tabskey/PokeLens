using PokeBattle.Core.Damage;

namespace PokeBattle.Core.Capture;

/// <summary>
/// For the Catch objective: the chance that one hit knocks the target out instead of weakening it.
/// Each of the 16 damage rolls is equally likely, so this is a calculated probability.
/// </summary>
public static class KoRiskCalculator
{
    /// <param name="damage">A result computed with <see cref="DamageRollPolicy.FullRange"/>.</param>
    public static double ChanceToKo(int currentHp, DamageResult damage)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(currentHp, 1);
        ArgumentNullException.ThrowIfNull(damage);

        if (damage.Rolls.Count == 0 && damage.CriticalRolls.Count == 0)
        {
            throw new ArgumentException("The damage result has no rolls; use DamageRollPolicy.FullRange.", nameof(damage));
        }

        var criticalChance = damage.CriticalHitChance;
        var nonCritical = criticalChance < 1 ? KoFraction(currentHp, damage.Rolls) : 0;
        var critical = criticalChance > 0 ? KoFraction(currentHp, damage.CriticalRolls) : 0;

        return ((1 - criticalChance) * nonCritical) + (criticalChance * critical);
    }

    private static double KoFraction(int currentHp, IReadOnlyList<int> rolls)
        => rolls.Count(roll => roll >= currentHp) / (double)rolls.Count;
}