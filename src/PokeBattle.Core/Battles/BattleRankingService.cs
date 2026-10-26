using PokeBattle.Core.Capture;

namespace PokeBattle.Core.Battles;

/// <summary>
/// Orders candidates by the criteria in docs/arquitetura.md §14, using the scenario the player
/// chose. Accessibility is not a criterion here: inaccessible candidates never reach the ranking.
/// </summary>
public static class BattleRankingService
{
    public const int DefaultLimit = 5;

    public static IReadOnlyList<CandidateEvaluation> Rank(
        IEnumerable<CandidateEvaluation> candidates,
        BattleObjective objective,
        ScenarioKind scenario,
        int limit = DefaultLimit)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);

        var ordered = objective switch
        {
            BattleObjective.Defeat => RankForDefeat(candidates, scenario),
            BattleObjective.Catch => RankForCatch(candidates, scenario),
            _ => throw new ArgumentOutOfRangeException(nameof(objective), objective, null),
        };

        return [.. ordered.ThenBy(candidate => candidate.Name, StringComparer.Ordinal).Take(limit)];
    }

    /// <summary>Matchup → Damage → Survivability → Turns to KO → Consistency.</summary>
    private static IOrderedEnumerable<CandidateEvaluation> RankForDefeat(IEnumerable<CandidateEvaluation> candidates, ScenarioKind scenario)
        => candidates
            .OrderBy(candidate => Simulation(candidate, scenario).Outcome)
            .ThenByDescending(candidate => DamageShare(Simulation(candidate, scenario).CandidateAttack))
            .ThenByDescending(candidate => Simulation(candidate, scenario).OpponentAttack?.TurnsToKo?.Min ?? int.MaxValue)
            .ThenBy(candidate => Simulation(candidate, scenario).CandidateAttack?.TurnsToKo?.Max ?? int.MaxValue)
            .ThenByDescending(candidate => candidate.Simulations.Count(simulation => simulation.Outcome == BattleOutcome.Favorable));

    /// <summary>
    /// Safe HP reduction → capture chance (Poké Ball) → KO risk. Status availability is skipped (no
    /// status moves in V1) and required balls follow the chance per ball, so they add nothing.
    /// </summary>
    private static IOrderedEnumerable<CandidateEvaluation> RankForCatch(IEnumerable<CandidateEvaluation> candidates, ScenarioKind scenario)
        => candidates
            .OrderByDescending(candidate => Plan(candidate, scenario).IsSafeHit)
            .ThenByDescending(candidate => Plan(candidate, scenario).Capture.GetValueOrDefault(PokeBall.PokeBall)?.ChancePerBall ?? 0)
            .ThenBy(candidate => Plan(candidate, scenario).KoRisk);

    private static ScenarioSimulation Simulation(CandidateEvaluation candidate, ScenarioKind scenario)
        => candidate.Simulations.Single(simulation => simulation.Scenario == scenario);

    private static CatchPlan Plan(CandidateEvaluation candidate, ScenarioKind scenario)
        => candidate.CatchPlans.Single(plan => plan.Scenario == scenario);

    /// <summary>Weakest roll as a fraction of the target's HP.</summary>
    private static double DamageShare(AttackOutcome? attack)
        => attack is null ? 0 : (double)attack.Damage.Min / attack.TargetMaxHp;
}