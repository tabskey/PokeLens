using PokeBattle.Core.Damage;
using PokeBattle.Core.Types;

namespace PokeBattle.Core.Battles;

/// <summary>
/// Simulates a single exchange between the candidate and the opponent under each scenario: both
/// sides use their best damaging move (the opponent included, as a conservative assumption) and
/// every move is assumed to hit. See docs/decisoes-simulacao.md.
/// </summary>
public static class BattleSimulationService
{
    public static IReadOnlyList<ScenarioSimulation> Simulate(
        BattlerSpec candidate,
        IReadOnlyList<Move> candidateMoves,
        BattlerSpec opponent,
        IReadOnlyList<Move> opponentMoves,
        DamageRollPolicy rollPolicy,
        CriticalHitPolicy criticalHitPolicy)
        => [.. ScenarioCatalog.All.Select(scenario =>
            SimulateScenario(candidate, candidateMoves, opponent, opponentMoves, scenario, rollPolicy, criticalHitPolicy))];

    public static ScenarioSimulation SimulateScenario(
        BattlerSpec candidate,
        IReadOnlyList<Move> candidateMoves,
        BattlerSpec opponent,
        IReadOnlyList<Move> opponentMoves,
        BattleScenario scenario,
        DamageRollPolicy rollPolicy,
        CriticalHitPolicy criticalHitPolicy)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(candidateMoves);
        ArgumentNullException.ThrowIfNull(opponent);
        ArgumentNullException.ThrowIfNull(opponentMoves);
        ArgumentNullException.ThrowIfNull(scenario);

        var (candidateBattler, candidateAttack) = BestAttack(
            candidate, BattleSide.Candidate, candidateMoves, opponent, BattleSide.Opponent, scenario, rollPolicy, criticalHitPolicy);
        var (opponentBattler, opponentAttack) = BestAttack(
            opponent, BattleSide.Opponent, opponentMoves, candidate, BattleSide.Candidate, scenario, rollPolicy, criticalHitPolicy);

        return new ScenarioSimulation(
            scenario.Kind,
            candidateBattler,
            opponentBattler,
            candidateAttack,
            opponentAttack,
            BattleOutcomeEvaluator.Classify(candidateAttack?.TurnsToKo),
            BattleOutcomeEvaluator.Evaluate(candidateAttack?.TurnsToKo, opponentAttack?.TurnsToKo));
    }

    /// <summary>
    /// Picks the move with the fewest worst-case turns to KO, then fewest best-case turns, then the
    /// highest maximum damage; remaining ties keep the order of the move list.
    /// </summary>
    private static (ResolvedBattler Attacker, AttackOutcome? Attack) BestAttack(
        BattlerSpec attackerSpec,
        BattleSide attackerSide,
        IReadOnlyList<Move> moves,
        BattlerSpec defenderSpec,
        BattleSide defenderSide,
        BattleScenario scenario,
        DamageRollPolicy rollPolicy,
        CriticalHitPolicy criticalHitPolicy)
    {
        // HP and defensive stats do not depend on the offensive category, so any category works here.
        var defender = ScenarioResolver.Resolve(defenderSpec, scenario, defenderSide, MoveCategory.Physical);

        var candidates = moves
            .Select(move =>
            {
                var attacker = ScenarioResolver.Resolve(attackerSpec, scenario, attackerSide, MoveCategoryResolver.GetCategory(move.Type));
                var damage = DamageCalculator.Calculate(attacker.Combatant, defender.Combatant, move, rollPolicy, criticalHitPolicy);
                var attack = new AttackOutcome(
                    move,
                    damage,
                    defender.MaxHp,
                    HpRange.AfterHit(defender.MaxHp, damage.Min, damage.Max),
                    TurnsToKo.From(defender.MaxHp, damage.Min, damage.Max));
                return (Attacker: attacker, Attack: attack);
            })
            .ToList();

        if (candidates.Count == 0)
        {
            return (ScenarioResolver.Resolve(attackerSpec, scenario, attackerSide, MoveCategory.Physical), null);
        }

        var best = candidates
            .OrderBy(candidate => candidate.Attack.TurnsToKo?.Max ?? int.MaxValue)
            .ThenBy(candidate => candidate.Attack.TurnsToKo?.Min ?? int.MaxValue)
            .ThenByDescending(candidate => candidate.Attack.Damage.Max)
            .First();

        return best;
    }
}