using PokeBattle.Core.Capture;

namespace PokeBattle.Core.Battles;

/// <summary>Everything simulated for one candidate: one entry per scenario in each list.</summary>
/// <param name="Name">Identifies the candidate; also the last tie-breaker, so the ranking is deterministic.</param>
/// <param name="CatchPlans">Empty for the Defeat objective.</param>
public sealed record CandidateEvaluation(
    string Name,
    IReadOnlyList<ScenarioSimulation> Simulations,
    IReadOnlyList<CatchPlan> CatchPlans);