namespace PokeBattle.Core.Battles;

/// <summary>
/// Derives labels from the simulated numbers. The win criterion is PROVISIONAL, pending review in
/// docs/decisoes-simulacao.md: Speed is not considered, so only non-overlapping turn ranges decide.
/// </summary>
public static class BattleOutcomeEvaluator
{
    public static KoClassification Classify(TurnsToKo? turnsToKo) => turnsToKo switch
    {
        null => KoClassification.NoDamage,
        { Max: 1 } => KoClassification.GuaranteedKo,
        { Min: 1 } => KoClassification.PossibleKo,
        { Min: 2 } => KoClassification.TwoHitKo,
        { Min: 3 } => KoClassification.ThreeHitKo,
        _ => KoClassification.FourOrMoreHits,
    };

    /// <summary>
    /// Favorable when the candidate's worst case knocks out before the opponent's best case (fewer
    /// turns wins regardless of who moves first); Unfavorable in the opposite situation; Balanced
    /// when the ranges overlap or tie, because the result then depends on rolls or Speed.
    /// </summary>
    public static BattleOutcome Evaluate(TurnsToKo? candidate, TurnsToKo? opponent)
    {
        if (candidate is null)
        {
            return BattleOutcome.Unfavorable;
        }

        if (opponent is null || candidate.Max < opponent.Min)
        {
            return BattleOutcome.Favorable;
        }

        return candidate.Min > opponent.Max ? BattleOutcome.Unfavorable : BattleOutcome.Balanced;
    }
}