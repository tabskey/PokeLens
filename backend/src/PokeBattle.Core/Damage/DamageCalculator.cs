using PokeBattle.Core.Types;

namespace PokeBattle.Core.Damage;

/// <summary>
/// Generation III damage formula, following the FireRed decompilation (pret/pokefirered):
/// CalculateBaseDamage → critical multiplier → STAB → type effectiveness → random factor. Integer
/// arithmetic only, truncating at each step in the same order as the game.
/// </summary>
/// <remarks>
/// Only Critical, STAB, Type and Random are modelled (V1). Burn, screens, weather, abilities, items,
/// stat stages and badge boosts are deliberately left out.
/// </remarks>
public static class DamageCalculator
{
    private const int RollCount = 16;
    private const int LowestRandomFactor = 85;
    private const double BaseCriticalHitChance = 1.0 / 16;
    private const double HighCriticalHitChance = 1.0 / 8;

    public static DamageResult Calculate(
        Combatant attacker,
        Combatant defender,
        Move move,
        DamageRollPolicy rollPolicy,
        CriticalHitPolicy criticalHitPolicy)
    {
        ArgumentNullException.ThrowIfNull(attacker);
        ArgumentNullException.ThrowIfNull(defender);
        ArgumentNullException.ThrowIfNull(move);

        var nonCritical = ComputeRolls(attacker, defender, move, criticalMultiplier: 1);
        var critical = ComputeRolls(attacker, defender, move, criticalMultiplier: 2);
        var effectiveness = TypeEffectivenessService.GetMultiplier(move.Type, defender.Type1, defender.Type2);

        var criticalHitChance = criticalHitPolicy switch
        {
            CriticalHitPolicy.Never => 0,
            CriticalHitPolicy.Guaranteed => 1,
            CriticalHitPolicy.Normal or CriticalHitPolicy.EvaluateProbability
                => move.HighCriticalRatio ? HighCriticalHitChance : BaseCriticalHitChance,
            _ => throw new ArgumentOutOfRangeException(nameof(criticalHitPolicy), criticalHitPolicy, null),
        };

        var (min, max) = criticalHitPolicy switch
        {
            CriticalHitPolicy.Guaranteed => (critical[0], critical[^1]),
            CriticalHitPolicy.Normal => (nonCritical[0], critical[^1]),
            _ => (nonCritical[0], nonCritical[^1]),
        };

        IReadOnlyList<int> rolls = criticalHitPolicy == CriticalHitPolicy.Guaranteed ? [] : nonCritical;
        IReadOnlyList<int> criticalRolls = criticalHitPolicy == CriticalHitPolicy.Never ? [] : critical;

        return rollPolicy switch
        {
            DamageRollPolicy.Minimum => new DamageResult(min, min, [], [], null, criticalHitChance, effectiveness),
            DamageRollPolicy.Maximum => new DamageResult(max, max, [], [], null, criticalHitChance, effectiveness),
            DamageRollPolicy.FullRange => new DamageResult(min, max, rolls, criticalRolls, null, criticalHitChance, effectiveness),
            DamageRollPolicy.Average => new DamageResult(
                min,
                max,
                [],
                [],
                ((1 - criticalHitChance) * nonCritical.Average()) + (criticalHitChance * critical.Average()),
                criticalHitChance,
                effectiveness),
            _ => throw new ArgumentOutOfRangeException(nameof(rollPolicy), rollPolicy, null),
        };
    }

    /// <summary>The 16 possible results, one per random factor 85..100, in ascending order.</summary>
    private static int[] ComputeRolls(Combatant attacker, Combatant defender, Move move, int criticalMultiplier)
    {
        var damage = CalculateBaseDamage(attacker, defender, move) * criticalMultiplier;

        if (attacker.HasType(move.Type))
        {
            damage = damage * 15 / 10;
        }

        damage = TypeEffectivenessService.ApplyTo(damage, move.Type, defender.Type1, defender.Type2);

        var rolls = new int[RollCount];
        for (var i = 0; i < RollCount; i++)
        {
            rolls[i] = ApplyRandomFactor(damage, LowestRandomFactor + i);
        }

        return rolls;
    }

    private static int CalculateBaseDamage(Combatant attacker, Combatant defender, Move move)
    {
        var category = MoveCategoryResolver.GetCategory(move.Type);
        var (attack, defense) = category == MoveCategory.Physical
            ? (attacker.Attack, defender.Defense)
            : (attacker.SpecialAttack, defender.SpecialDefense);

        var damage = attack * move.Power;
        damage *= (2 * attacker.Level / 5) + 2;
        damage /= defense;
        damage /= 50;

        // Only the physical branch of CalculateBaseDamage has the "at least 1" floor.
        if (category == MoveCategory.Physical && damage == 0)
        {
            damage = 1;
        }

        return damage + 2;
    }

    private static int ApplyRandomFactor(int damage, int randomFactor)
    {
        if (damage == 0)
        {
            return 0;
        }

        return Math.Max(1, damage * randomFactor / 100);
    }
}