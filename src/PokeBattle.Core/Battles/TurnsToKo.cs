namespace PokeBattle.Core.Battles;

/// <summary>
/// How many hits the attacker needs to knock the target out, from the best case (every hit at
/// maximum damage) to the worst case (every hit at minimum damage).
/// </summary>
public sealed record TurnsToKo(int Min, int Max)
{
    /// <summary>Returns null when the attack deals no damage (e.g. type immunity).</summary>
    public static TurnsToKo? From(int targetHp, int minDamage, int maxDamage)
    {
        if (minDamage <= 0)
        {
            return null;
        }

        return new TurnsToKo(CeilingDivide(targetHp, maxDamage), CeilingDivide(targetHp, minDamage));
    }

    private static int CeilingDivide(int dividend, int divisor) => (dividend + divisor - 1) / divisor;
}