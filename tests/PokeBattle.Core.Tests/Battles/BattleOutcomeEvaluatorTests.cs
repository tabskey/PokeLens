using PokeBattle.Core.Battles;

namespace PokeBattle.Core.Tests.Battles;

// The win criterion is PROVISIONAL (docs/decisoes-simulacao.md): no Speed, only non-overlapping
// turn ranges decide; overlaps and exact ties are Balanced.
public class BattleOutcomeEvaluatorTests
{
    [Theory]
    [InlineData(2, 2, 3, 4, BattleOutcome.Favorable)]    // worst case 2 turns < opponent's best case 3
    [InlineData(2, 3, 3, 4, BattleOutcome.Balanced)]     // ranges overlap on 3
    [InlineData(2, 2, 2, 2, BattleOutcome.Balanced)]     // exact tie: depends on who moves first
    [InlineData(3, 4, 2, 2, BattleOutcome.Unfavorable)]  // best case 3 turns > opponent's worst case 2
    [InlineData(1, 1, 2, 5, BattleOutcome.Favorable)]
    public void Evaluate_ComparesTurnRanges(int candidateMin, int candidateMax, int opponentMin, int opponentMax, BattleOutcome expected)
    {
        var outcome = BattleOutcomeEvaluator.Evaluate(new TurnsToKo(candidateMin, candidateMax), new TurnsToKo(opponentMin, opponentMax));

        Assert.Equal(expected, outcome);
    }

    [Fact]
    public void Evaluate_CandidateDealsNoDamage_IsUnfavorable()
    {
        Assert.Equal(BattleOutcome.Unfavorable, BattleOutcomeEvaluator.Evaluate(null, new TurnsToKo(5, 7)));
    }

    [Fact]
    public void Evaluate_OpponentDealsNoDamage_IsFavorable()
    {
        Assert.Equal(BattleOutcome.Favorable, BattleOutcomeEvaluator.Evaluate(new TurnsToKo(9, 12), null));
    }

    [Fact]
    public void Evaluate_NeitherDealsDamage_IsUnfavorable()
    {
        Assert.Equal(BattleOutcome.Unfavorable, BattleOutcomeEvaluator.Evaluate(null, null));
    }

    [Theory]
    [InlineData(1, 1, KoClassification.GuaranteedKo)]
    [InlineData(1, 2, KoClassification.PossibleKo)]
    [InlineData(2, 2, KoClassification.TwoHitKo)]
    [InlineData(2, 3, KoClassification.TwoHitKo)]
    [InlineData(3, 4, KoClassification.ThreeHitKo)]
    [InlineData(4, 4, KoClassification.FourOrMoreHits)]
    [InlineData(5, 7, KoClassification.FourOrMoreHits)]
    public void Classify_UsesTheTurnRange(int min, int max, KoClassification expected)
    {
        Assert.Equal(expected, BattleOutcomeEvaluator.Classify(new TurnsToKo(min, max)));
    }

    [Fact]
    public void Classify_NoDamage()
    {
        Assert.Equal(KoClassification.NoDamage, BattleOutcomeEvaluator.Classify(null));
    }

    // 20 HP: max damage 10 -> ceil(20/10) = 2; min damage 8 -> ceil(20/8) = 3.
    [Fact]
    public void TurnsToKo_RoundsUp()
    {
        Assert.Equal(new TurnsToKo(2, 3), TurnsToKo.From(20, minDamage: 8, maxDamage: 10));
    }

    [Fact]
    public void TurnsToKo_NoDamage_IsNull()
    {
        Assert.Null(TurnsToKo.From(20, minDamage: 0, maxDamage: 0));
    }

    [Fact]
    public void HpRange_NeverGoesBelowZero()
    {
        Assert.Equal(new HpRange(0, 1), HpRange.AfterHit(31, minDamage: 30, maxDamage: 36));
    }
}