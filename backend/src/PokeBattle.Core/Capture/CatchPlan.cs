using PokeBattle.Core.Battles;

namespace PokeBattle.Core.Capture;

/// <summary>
/// How a candidate prepares the target for capture in one scenario: one or more hits, then balls.
/// </summary>
/// <param name="Hit">The chosen hit; null when the candidate has no move that damages the target.</param>
/// <param name="Hits">Times the hit is used: every safe hit, or 1 when none is safe; 0 when there is no hit.</param>
/// <param name="KoRisk">Chance that the hits knock the target out (calculated, critical hits included); 0 for safe hits.</param>
/// <param name="TargetHp">Target HP after the weakest roll of every hit (the worst case for capture); 0 when every roll knocks out.</param>
/// <param name="Capture">Capture odds per ball at <paramref name="TargetHp"/>, no status; empty when TargetHp is 0.</param>
public sealed record CatchPlan(
    ScenarioKind Scenario,
    AttackOutcome? Hit,
    int Hits,
    double KoRisk,
    int TargetHp,
    IReadOnlyDictionary<PokeBall, CaptureResult> Capture)
{
    /// <summary>The hits lower the target's HP and can never knock it out.</summary>
    public bool IsSafeHit => Hit is not null && KoRisk == 0;
}