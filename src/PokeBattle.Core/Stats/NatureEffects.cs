namespace PokeBattle.Core.Stats;

/// <summary>
/// Maps each Nature to the stat it raises and the stat it lowers. The five natures where both are
/// the same stat (Hardy, Docile, Serious, Bashful, Quirky) leave every stat untouched, so they are
/// recorded as "no effect" (null, null) rather than a self-cancelling pair.
/// </summary>
public static class NatureEffects
{
    private static readonly Dictionary<Nature, (StatType? Increased, StatType? Decreased)> Effects = new()
    {
        [Nature.Hardy] = (null, null),
        [Nature.Lonely] = (StatType.Attack, StatType.Defense),
        [Nature.Brave] = (StatType.Attack, StatType.Speed),
        [Nature.Adamant] = (StatType.Attack, StatType.SpecialAttack),
        [Nature.Naughty] = (StatType.Attack, StatType.SpecialDefense),
        [Nature.Bold] = (StatType.Defense, StatType.Attack),
        [Nature.Docile] = (null, null),
        [Nature.Relaxed] = (StatType.Defense, StatType.Speed),
        [Nature.Impish] = (StatType.Defense, StatType.SpecialAttack),
        [Nature.Lax] = (StatType.Defense, StatType.SpecialDefense),
        [Nature.Timid] = (StatType.Speed, StatType.Attack),
        [Nature.Hasty] = (StatType.Speed, StatType.Defense),
        [Nature.Serious] = (null, null),
        [Nature.Jolly] = (StatType.Speed, StatType.SpecialAttack),
        [Nature.Naive] = (StatType.Speed, StatType.SpecialDefense),
        [Nature.Modest] = (StatType.SpecialAttack, StatType.Attack),
        [Nature.Mild] = (StatType.SpecialAttack, StatType.Defense),
        [Nature.Quiet] = (StatType.SpecialAttack, StatType.Speed),
        [Nature.Bashful] = (null, null),
        [Nature.Rash] = (StatType.SpecialAttack, StatType.SpecialDefense),
        [Nature.Calm] = (StatType.SpecialDefense, StatType.Attack),
        [Nature.Gentle] = (StatType.SpecialDefense, StatType.Defense),
        [Nature.Sassy] = (StatType.SpecialDefense, StatType.Speed),
        [Nature.Careful] = (StatType.SpecialDefense, StatType.SpecialAttack),
        [Nature.Quirky] = (null, null),
    };

    public static NatureEffect GetEffect(Nature nature, StatType statType)
    {
        var (increased, decreased) = Effects[nature];
        if (increased == statType)
        {
            return NatureEffect.Raised;
        }

        if (decreased == statType)
        {
            return NatureEffect.Lowered;
        }

        return NatureEffect.Neutral;
    }

    /// <summary>Returns the Nature modifier as an exact integer fraction (110/100, 90/100 or 100/100).</summary>
    public static (int Numerator, int Denominator) GetMultiplier(Nature nature, StatType statType)
        => GetMultiplier(GetEffect(nature, statType));

    public static (int Numerator, int Denominator) GetMultiplier(NatureEffect effect) => effect switch
    {
        NatureEffect.Raised => (110, 100),
        NatureEffect.Lowered => (90, 100),
        NatureEffect.Neutral => (100, 100),
        _ => throw new ArgumentOutOfRangeException(nameof(effect), effect, null),
    };
}