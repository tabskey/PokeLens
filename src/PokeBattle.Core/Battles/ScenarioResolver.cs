using PokeBattle.Core.Damage;
using PokeBattle.Core.Stats;
using PokeBattle.Core.Types;

namespace PokeBattle.Core.Battles;

/// <summary>
/// Turns a <see cref="BattlerSpec"/> into concrete stats under a scenario. Known and UserProvided
/// attributes are used as they are; only Estimated ones take the scenario's assumption.
/// </summary>
public static class ScenarioResolver
{
    /// <param name="offensiveCategory">
    /// Category of the move this side attacks with. An Estimated Nature only affects the matching
    /// offensive stat (Attack or Sp. Atk); every other stat stays neutral.
    /// </param>
    public static ResolvedBattler Resolve(BattlerSpec spec, BattleScenario scenario, BattleSide side, MoveCategory offensiveCategory)
    {
        ArgumentNullException.ThrowIfNull(spec);
        ArgumentNullException.ThrowIfNull(scenario);

        var profile = scenario.For(side);
        var level = ResolveLevel(spec.Level, profile.Level);
        var ivs = spec.IndividualValues ?? StatSpread.Uniform(ScenarioCatalog.IndividualValueFor(profile.Iv));
        var evs = spec.EffortValues ?? StatSpread.Uniform(ScenarioCatalog.EffortValueFor(profile.Ev));
        var offensiveStat = offensiveCategory == MoveCategory.Physical ? StatType.Attack : StatType.SpecialAttack;

        int Stat(StatType statType, int baseStat, int iv, int ev)
        {
            var effect = spec.Nature is { } nature
                ? NatureEffects.GetEffect(nature, statType)
                : statType == offensiveStat ? ToEffect(profile.Nature) : NatureEffect.Neutral;
            return StatCalculator.CalculateStat(baseStat, iv, ev, level, effect);
        }

        var b = spec.BaseStats;
        var combatant = new Combatant(
            level,
            Attack: Stat(StatType.Attack, b.Attack, ivs.Attack, evs.Attack),
            Defense: Stat(StatType.Defense, b.Defense, ivs.Defense, evs.Defense),
            SpecialAttack: Stat(StatType.SpecialAttack, b.SpecialAttack, ivs.SpecialAttack, evs.SpecialAttack),
            SpecialDefense: Stat(StatType.SpecialDefense, b.SpecialDefense, ivs.SpecialDefense, evs.SpecialDefense),
            spec.Type1,
            spec.Type2);

        var maxHp = StatCalculator.CalculateHp(b.Hp, ivs.Hp, evs.Hp, level);
        return new ResolvedBattler(level, maxHp, combatant);
    }

    private static int ResolveLevel(LevelRange range, LevelProfile profile) => profile switch
    {
        LevelProfile.Lowest => range.Min,
        LevelProfile.Midpoint => (range.Min + range.Max) / 2,
        LevelProfile.Highest => range.Max,
        _ => throw new ArgumentOutOfRangeException(nameof(profile), profile, null),
    };

    private static NatureEffect ToEffect(NatureProfile profile) => profile switch
    {
        NatureProfile.Hindering => NatureEffect.Lowered,
        NatureProfile.Neutral => NatureEffect.Neutral,
        NatureProfile.Favoring => NatureEffect.Raised,
        _ => throw new ArgumentOutOfRangeException(nameof(profile), profile, null),
    };
}