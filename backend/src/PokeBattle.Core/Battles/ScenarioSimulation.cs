namespace PokeBattle.Core.Battles;

/// <summary>Result of the battle simulation for one scenario.</summary>
/// <param name="CandidateAttack">Null when the candidate has no damaging move.</param>
/// <param name="OpponentAttack">Null when the opponent has no damaging move.</param>
public sealed record ScenarioSimulation(
    ScenarioKind Scenario,
    ResolvedBattler Candidate,
    ResolvedBattler Opponent,
    AttackOutcome? CandidateAttack,
    AttackOutcome? OpponentAttack,
    KoClassification CandidateKo,
    BattleOutcome Outcome);