using PokeBattle.Core.Battles;
using PokeBattle.Core.Damage;
using PokeBattle.Core.Types;

namespace PokeBattle.Core.Capture;

/// <summary>
/// V1 capture strategy: hit the target once with the strongest move that cannot knock it out from
/// full HP (if every move can, the one with the lowest KO risk), then throw balls at the HP left by
/// the weakest roll. Status moves are not considered.
/// </summary>
public static class CatchPlanner
{
    private static readonly PokeBall[] Balls = [PokeBall.PokeBall, PokeBall.GreatBall, PokeBall.UltraBall];

    public static CatchPlan Plan(
        BattlerSpec candidate,
        IReadOnlyList<Move> candidateMoves,
        BattlerSpec target,
        int targetCatchRate,
        BattleScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(candidateMoves);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(scenario);

        // HP and defensive stats do not depend on the offensive category, so any category works here.
        var defender = ScenarioResolver.Resolve(target, scenario, BattleSide.Opponent, MoveCategory.Physical);

        var hit = candidateMoves
            .Select(move =>
            {
                var attacker = ScenarioResolver.Resolve(candidate, scenario, BattleSide.Candidate, MoveCategoryResolver.GetCategory(move.Type));
                var damage = DamageCalculator.Calculate(attacker.Combatant, defender.Combatant, move, DamageRollPolicy.FullRange, CriticalHitPolicy.EvaluateProbability);
                return (Damage: damage, Move: move);
            })
            .Where(option => option.Damage.Max > 0)
            .Select(option => new
            {
                Attack = new AttackOutcome(
                    option.Move,
                    option.Damage,
                    defender.MaxHp,
                    HpRange.AfterHit(defender.MaxHp, option.Damage.Min, option.Damage.Max),
                    TurnsToKo.From(defender.MaxHp, option.Damage.Min, option.Damage.Max)),
                KoRisk = KoRiskCalculator.ChanceToKo(defender.MaxHp, option.Damage),
            })
            .OrderBy(option => option.KoRisk)
            .ThenByDescending(option => option.Attack.Damage.Min)
            .ThenByDescending(option => option.Attack.Damage.Max)
            .FirstOrDefault();

        var targetHp = hit?.Attack.RemainingHp.Max ?? defender.MaxHp;
        var capture = targetHp == 0
            ? new Dictionary<PokeBall, CaptureResult>()
            : Balls.ToDictionary(ball => ball, ball => CaptureCalculator.Calculate(targetCatchRate, defender.MaxHp, targetHp, ball, CaptureStatus.None));

        return new CatchPlan(scenario.Kind, hit?.Attack, hit?.KoRisk ?? 0, targetHp, capture);
    }
}