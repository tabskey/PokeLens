using PokeBattle.Core.Damage;

namespace PokeBattle.Core.Battles;

/// <summary>One side attacking the other with its best move in a scenario.</summary>
/// <param name="TurnsToKo">Null when the move deals no damage.</param>
public sealed record AttackOutcome(Move Move, DamageResult Damage, int TargetMaxHp, HpRange RemainingHp, TurnsToKo? TurnsToKo);