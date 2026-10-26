namespace PokeBattle.Core.Damage;

/// <summary>
/// Result of a damage calculation. A damage range is not a win probability: 79–81 means the hit
/// deals 79, 80 or 81 damage.
/// </summary>
/// <param name="Min">Lowest damage under the chosen policies.</param>
/// <param name="Max">Highest damage under the chosen policies.</param>
/// <param name="Rolls">The 16 non-critical results (factor 85..100), only for <see cref="DamageRollPolicy.FullRange"/>.</param>
/// <param name="CriticalRolls">The 16 critical results, only for <see cref="DamageRollPolicy.FullRange"/> when a critical hit is considered.</param>
/// <param name="Average">Mean damage, only for <see cref="DamageRollPolicy.Average"/>.</param>
/// <param name="CriticalHitChance">0 for Never, 1 for Guaranteed, otherwise the Generation III chance.</param>
/// <param name="Effectiveness">Combined type multiplier (0, 0.25, 0.5, 1, 2 or 4).</param>
public sealed record DamageResult(
    int Min,
    int Max,
    IReadOnlyList<int> Rolls,
    IReadOnlyList<int> CriticalRolls,
    double? Average,
    double CriticalHitChance,
    double Effectiveness);