using PokeBattle.Core.Battles;

namespace PokeBattle.Core.Capture;

/// <summary>
/// How a candidate prepares the target for capture in one scenario: a single hit, then balls.
/// </summary>
/// <param name="Hit">The chosen hit; null when the candidate has no move that damages the target.</param>
/// <param name="KoRisk">Chance that the hit knocks the target out (calculated, critical hits included).</param>
/// <param name="TargetHp">Target HP after the weakest roll of the hit (the worst case for capture); 0 when every roll knocks out.</param>
/// <param name="Capture">Capture odds per ball at <paramref name="TargetHp"/>, no status; empty when TargetHp is 0.</param>
public sealed record CatchPlan(
    ScenarioKind Scenario,
    AttackOutcome? Hit,
    double KoRisk,
    int TargetHp,
    IReadOnlyDictionary<PokeBall, CaptureResult> Capture)
{
    /// <summary>The hit lowers the target's HP and can never knock it out.</summary>
    public bool IsSafeHit => Hit is not null && KoRisk == 0;
}