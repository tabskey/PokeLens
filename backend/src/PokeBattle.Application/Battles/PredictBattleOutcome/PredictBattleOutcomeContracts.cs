using PokeBattle.Core.Battles;
using PokeBattle.Core.Capture;

namespace PokeBattle.Application.Battles.PredictBattleOutcome;

/// <param name="Target">Species name as in the data package (e.g. "pidgey").</param>
/// <param name="Area">Location area where the player meets the target (e.g. "kanto-route-1-area").</param>
/// <param name="Version">"firered" or "leafgreen".</param>
/// <param name="Milestone">How far the player is, one of the progression milestones (e.g. "boulder-badge").</param>
/// <param name="Scenario">Scenario that orders the ranking: Pessimistic (safest), Neutral or Optimistic.</param>
public sealed record PredictBattleOutcomeRequest(
    string Target,
    string Area,
    string Version,
    string Milestone,
    BattleObjective Objective,
    ScenarioKind Scenario);

public sealed record PredictBattleOutcomeResponse(
    string Target,
    int TargetMinLevel,
    int TargetMaxLevel,
    BattleObjective Objective,
    ScenarioKind RankedBy,
    IReadOnlyList<CandidateResult> Candidates);

/// <summary>A recommended Pokémon and where the player gets it.</summary>
public sealed record CandidateResult(
    string Pokemon,
    string Area,
    string Method,
    int MinLevel,
    int MaxLevel,
    IReadOnlyList<ScenarioResult> Scenarios);

/// <param name="Catch">Only for the Catch objective.</param>
public sealed record ScenarioResult(
    ScenarioKind Scenario,
    AttackResult? CandidateAttack,
    AttackResult? OpponentAttack,
    KoClassification CandidateKo,
    BattleOutcome Outcome,
    CatchResult? Catch);

/// <summary>Damage and remaining HP are ranges, not probabilities. TurnsToKo is null when the move deals no damage.</summary>
public sealed record AttackResult(
    string Move,
    int MinDamage,
    int MaxDamage,
    int TargetMaxHp,
    int RemainingHpMin,
    int RemainingHpMax,
    int? TurnsToKoMin,
    int? TurnsToKoMax);

/// <param name="Hit">Null when no move damages the target (balls are thrown at full HP).</param>
/// <param name="KoRisk">Calculated chance that the hit knocks the target out.</param>
/// <param name="TargetHp">HP after the weakest roll; balls are evaluated at this HP.</param>
public sealed record CatchResult(AttackResult? Hit, double KoRisk, int TargetHp, IReadOnlyList<BallResult> Balls);

/// <summary>ChancePerBall is per throw; the BallsFor* values are throws needed for that cumulative chance.</summary>
public sealed record BallResult(
    PokeBall Ball,
    double ChancePerBall,
    int BallsFor50Percent,
    int BallsFor90Percent,
    int BallsFor95Percent,
    int BallsFor99Percent);