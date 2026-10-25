namespace PokeBattle.Core.Stats;

/// <summary>
/// Generation III stat formulas. Integer arithmetic only, truncating at each step exactly like the
/// original game (C#'s integer division already truncates toward zero, which equals floor here
/// since every input is non-negative).
/// </summary>
public static class StatCalculator
{
    public static int CalculateHp(int baseStat, int individualValue, int effortValue, int level)
    {
        var levelScaled = ScaleByLevel(baseStat, individualValue, effortValue, level);
        return levelScaled + level + 10;
    }

    public static int CalculateStat(int baseStat, int individualValue, int effortValue, int level, StatType statType, Nature nature)
        => CalculateStat(baseStat, individualValue, effortValue, level, NatureEffects.GetEffect(nature, statType));

    /// <summary>
    /// Same formula with the Nature effect given directly, for when the exact Nature is unknown but
    /// its effect on this stat is assumed (e.g. a battle scenario for a wild Pokémon).
    /// </summary>
    public static int CalculateStat(int baseStat, int individualValue, int effortValue, int level, NatureEffect natureEffect)
    {
        var levelScaled = ScaleByLevel(baseStat, individualValue, effortValue, level);
        var preNature = levelScaled + 5;
        var (numerator, denominator) = NatureEffects.GetMultiplier(natureEffect);
        return preNature * numerator / denominator;
    }

    private static int ScaleByLevel(int baseStat, int individualValue, int effortValue, int level)
    {
        var preLevel = (2 * baseStat) + individualValue + (effortValue / 4);
        return preLevel * level / 100;
    }
}