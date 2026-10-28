using PokeBattle.Core.Battles;
using PokeBattle.Core.Damage;
using PokeBattle.Core.Types;

namespace PokeBattle.Core.Capture;

/// <summary>
/// V1 capture strategy: hit the target as many times as the move can never knock it out, critical
/// hits included, and pick the move that leaves the least HP that way. If every move can knock out
/// in one hit, hit once with the one with the lowest KO risk. Balls are thrown at the HP left by the
/// weakest rolls. Status moves are not considered.
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
            .Select(option =>
            {
                // Even the highest roll (critical included) this many times leaves at least 1 HP.
                var highest = option.Damage.CriticalHitChance > 0 ? option.Damage.CriticalRolls.Max() : option.Damage.Rolls.Max();
                var safeHits = (defender.MaxHp - 1) / highest;
                var hits = Math.Max(1, safeHits);

                return new
                {
                    Attack = new AttackOutcome(
                        option.Move,
                        option.Damage,
                        defender.MaxHp,
                        HpRange.AfterHit(defender.MaxHp, option.Damage.Min, option.Damage.Max),
                        TurnsToKo.From(defender.MaxHp, option.Damage.Min, option.Damage.Max)),
                    Hits = hits,
                    KoRisk = safeHits > 0 ? 0 : KoRiskCalculator.ChanceToKo(defender.MaxHp, option.Damage),
                    TargetHp = Math.Max(0, defender.MaxHp - (hits * option.Damage.Min)),
                };
            })
            .OrderBy(option => option.KoRisk)
            .ThenBy(option => option.TargetHp)
            .ThenByDescending(option => option.Attack.Damage.Max)
            .FirstOrDefault();

        var targetHp = hit?.TargetHp ?? defender.MaxHp;
        var capture = targetHp == 0
            ? new Dictionary<PokeBall, CaptureResult>()
            : Balls.ToDictionary(ball => ball, ball => CaptureCalculator.Calculate(targetCatchRate, defender.MaxHp, targetHp, ball, CaptureStatus.None));

        return new CatchPlan(scenario.Kind, hit?.Attack, hit?.Hits ?? 0, hit?.KoRisk ?? 0, targetHp, capture);
    }
}