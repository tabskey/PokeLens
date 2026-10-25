using PokeBattle.Core.Damage;
using PokeBattle.Core.Types;

namespace PokeBattle.Core.Tests.Damage;

// Reference values: @smogon/calc 0.12.0 (the Pokémon Showdown damage calculator), Generation 3,
// no items, EVs 0, every IV set to the value noted in each case. The stats below are the ones the
// calculator derived for those species, IVs, natures and levels. Each case also shows the manual
// calculation following the pret/pokefirered order:
//   base = floor(floor(A * Power * (2L/5 + 2) / D) / 50) [+ physical floor of 1] + 2
//   then x2 critical, x15/10 STAB, x(type tenths)/10 per chart entry, x(85..100)/100.
public class DamageCalculatorTests
{
    // Case 1 — Rattata L5 (IV 20, Hardy) Tackle vs Pidgey L5 (IV 10, Hardy). Physical, STAB, neutral.
    // Atk 11, Def 9, Power 35. 2*5/5+2 = 4.
    // 11*35 = 385; *4 = 1540; /9 = 171; /50 = 3; +2 = 5. STAB: 5*15/10 = 7. Normal vs Normal/Flying = 1x.
    // Non-critical: 7*85/100 = 5 ... 7*100/100 = 7.   Critical: 10 -> STAB 15 -> 12 ... 15.
    [Fact]
    public void Calculate_PhysicalStabNeutral_MatchesReference()
    {
        var attacker = new Combatant(5, Attack: 11, Defense: 9, SpecialAttack: 8, SpecialDefense: 9, PokemonType.Normal);
        var defender = new Combatant(5, Attack: 10, Defense: 9, SpecialAttack: 9, SpecialDefense: 9, PokemonType.Normal, PokemonType.Flying);

        AssertMatchesReference(
            attacker,
            defender,
            new Move(PokemonType.Normal, 35),
            [5, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 7],
            [12, 12, 13, 13, 13, 13, 13, 13, 13, 14, 14, 14, 14, 14, 14, 15]);
    }

    // Case 2 — Charmander L10 (IV 31, Modest) Ember vs Bulbasaur L10 (IV 15, Hardy). Special, STAB, 2x.
    // SpA 22, SpD 19, Power 40. 2*10/5+2 = 6.
    // 22*40 = 880; *6 = 5280; /19 = 277; /50 = 5; +2 = 7. STAB: 10. Fire->Grass x20/10 = 20; Fire->Poison neutral.
    // Non-critical: 17 ... 20.   Critical: 14 -> STAB 21 -> 42 -> 35 ... 42.
    [Fact]
    public void Calculate_SpecialStabSuperEffective_MatchesReference()
    {
        var attacker = new Combatant(10, Attack: 16, Defense: 16, SpecialAttack: 22, SpecialDefense: 18, PokemonType.Fire);
        var defender = new Combatant(10, Attack: 16, Defense: 16, SpecialAttack: 19, SpecialDefense: 19, PokemonType.Grass, PokemonType.Poison);

        AssertMatchesReference(
            attacker,
            defender,
            new Move(PokemonType.Fire, 40),
            [17, 17, 17, 17, 17, 18, 18, 18, 18, 18, 19, 19, 19, 19, 19, 20],
            [35, 36, 36, 36, 37, 37, 38, 38, 39, 39, 39, 40, 40, 41, 41, 42]);
    }

    // Case 3 — Squirtle L14 (IV 25, Hardy) Water Gun vs Geodude L14 (IV 10, Hardy). Special, STAB, 4x.
    // SpA 22, SpD 14, Power 40. 2*14/5+2 = 7.
    // 22*40 = 880; *7 = 6160; /14 = 440; /50 = 8; +2 = 10. STAB: 15. Water->Ground 30; Water->Rock 60.
    // Non-critical: 51 ... 60.   Critical: 20 -> 30 -> 120 -> 102 ... 120.
    [Fact]
    public void Calculate_DualTypeQuadrupleEffective_MatchesReference()
    {
        var attacker = new Combatant(14, Attack: 21, Defense: 26, SpecialAttack: 22, SpecialDefense: 26, PokemonType.Water);
        var defender = new Combatant(14, Attack: 28, Defense: 34, SpecialAttack: 14, SpecialDefense: 14, PokemonType.Rock, PokemonType.Ground);

        AssertMatchesReference(
            attacker,
            defender,
            new Move(PokemonType.Water, 40),
            [51, 51, 52, 52, 53, 54, 54, 55, 55, 56, 57, 57, 58, 58, 59, 60],
            [102, 103, 104, 105, 106, 108, 109, 110, 111, 112, 114, 115, 116, 117, 118, 120]);
    }

    // Case 4 — Raticate L20 (IV 15, Hardy) Bite vs Gastly L20 (IV 20, Hardy). Bite is Dark, therefore
    // SPECIAL in Generation III: it uses SpA/SpD, not Atk/Def. No STAB, 2x.
    // SpA 28, SpD 23, Power 60. 2*20/5+2 = 10.
    // 28*60 = 1680; *10 = 16800; /23 = 730; /50 = 14; +2 = 16. Dark->Ghost 32; Dark->Poison neutral.
    // Non-critical: 27 ... 32.   Critical: 32 -> 64 -> 54 ... 64.
    [Fact]
    public void Calculate_BiteIsSpecial_MatchesReference()
    {
        var attacker = new Combatant(20, Attack: 40, Defense: 32, SpecialAttack: 28, SpecialDefense: 36, PokemonType.Normal);
        var defender = new Combatant(20, Attack: 23, Defense: 21, SpecialAttack: 49, SpecialDefense: 23, PokemonType.Ghost, PokemonType.Poison);

        AssertMatchesReference(
            attacker,
            defender,
            new Move(PokemonType.Dark, 60),
            [27, 27, 27, 28, 28, 28, 29, 29, 29, 30, 30, 30, 31, 31, 31, 32],
            [54, 55, 55, 56, 56, 57, 58, 58, 59, 60, 60, 61, 62, 62, 63, 64]);
    }

    // Case 5 — Bulbasaur L15 (IV 31, Hardy) Vine Whip (Gen III power 35) vs Charmander L15 (IV 31,
    // Hardy). Special, STAB, 0.5x.
    // SpA 29, SpD 24, Power 35. 2*15/5+2 = 8.
    // 29*35 = 1015; *8 = 8120; /24 = 338; /50 = 6; +2 = 8. STAB: 12. Grass->Fire x5/10 = 6.
    // Non-critical: 5 ... 6.   Critical: 16 -> 24 -> 12 -> 10 ... 12.
    [Fact]
    public void Calculate_NotVeryEffective_MatchesReference()
    {
        var attacker = new Combatant(15, Attack: 24, Defense: 24, SpecialAttack: 29, SpecialDefense: 29, PokemonType.Grass, PokemonType.Poison);
        var defender = new Combatant(15, Attack: 25, Defense: 22, SpecialAttack: 27, SpecialDefense: 24, PokemonType.Fire);

        AssertMatchesReference(
            attacker,
            defender,
            new Move(PokemonType.Grass, 35),
            [5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 6],
            [10, 10, 10, 10, 10, 10, 10, 11, 11, 11, 11, 11, 11, 11, 11, 12]);
    }

    // Case 6 — Mankey L12 (IV 18, Adamant) Karate Chop vs Pidgey L12 (IV 5, Hardy). Physical, STAB,
    // 2x and 0.5x on a dual type (chart order: Fighting->Normal before Fighting->Flying).
    // Atk 28, Def 15, Power 50. 2*12/5+2 = 6.
    // 28*50 = 1400; *6 = 8400; /15 = 560; /50 = 11; +2 = 13. STAB: 19. ->Normal 38; ->Flying 19.
    // Non-critical: 16 ... 19.   Critical: 26 -> 39 -> 78 -> 39 -> 33 ... 39.
    [Fact]
    public void Calculate_MixedEffectivenessOnDualType_MatchesReference()
    {
        var attacker = new Combatant(12, Attack: 28, Defense: 15, SpecialAttack: 13, SpecialDefense: 17, PokemonType.Fighting);
        var defender = new Combatant(12, Attack: 16, Defense: 15, SpecialAttack: 14, SpecialDefense: 14, PokemonType.Normal, PokemonType.Flying);

        AssertMatchesReference(
            attacker,
            defender,
            new Move(PokemonType.Fighting, 50, highCriticalRatio: true),
            [16, 16, 16, 16, 16, 17, 17, 17, 17, 17, 18, 18, 18, 18, 18, 19],
            [33, 33, 33, 34, 34, 35, 35, 35, 36, 36, 37, 37, 37, 38, 38, 39]);
    }

    // Case 7 — Lapras L26 (IV 10, Hardy) Ice Beam vs Moltres L50 (IV 31, Hardy). Here the game's chart
    // order matters and @smogon/calc 0.12.0 diverges from the game.
    // SpA 51, SpD 105, Power 95. 2*26/5+2 = 12.
    // 51*95 = 4845; *12 = 58140; /105 = 553; /50 = 11; +2 = 13. STAB: 19.
    // pret/pokefirered gTypeEffectiveness lists Ice->Flying (2x) BEFORE Ice->Fire (0.5x):
    //   19*20/10 = 38; 38*5/10 = 19 -> rolls 16 ... 19.
    // @smogon/calc orders by defender type (Fire before Flying): 19*0.5 = 9; 9*2 = 18 -> 15 ... 18.
    [Fact]
    public void Calculate_AppliesTypeChartInGameOrder()
    {
        var attacker = new Combatant(26, Attack: 51, Defense: 49, SpecialAttack: 51, SpecialDefense: 56, PokemonType.Water, PokemonType.Ice);
        var defender = new Combatant(50, Attack: 120, Defense: 110, SpecialAttack: 145, SpecialDefense: 105, PokemonType.Fire, PokemonType.Flying);

        var result = DamageCalculator.Calculate(attacker, defender, new Move(PokemonType.Ice, 95), DamageRollPolicy.FullRange, CriticalHitPolicy.Never);

        Assert.Equal([16, 16, 16, 16, 16, 17, 17, 17, 17, 17, 18, 18, 18, 18, 18, 19], result.Rolls);
    }

    [Fact]
    public void Calculate_Minimum_ReturnsLowestNonCriticalRoll()
    {
        var result = DamageCalculator.Calculate(CharmanderL10, BulbasaurL10, Ember, DamageRollPolicy.Minimum, CriticalHitPolicy.Never);

        Assert.Equal(17, result.Min);
        Assert.Equal(17, result.Max);
        Assert.Empty(result.Rolls);
        Assert.Empty(result.CriticalRolls);
        Assert.Null(result.Average);
        Assert.Equal(0, result.CriticalHitChance);
        Assert.Equal(2, result.Effectiveness);
    }

    [Fact]
    public void Calculate_Maximum_ReturnsHighestNonCriticalRoll()
    {
        var result = DamageCalculator.Calculate(CharmanderL10, BulbasaurL10, Ember, DamageRollPolicy.Maximum, CriticalHitPolicy.Never);

        Assert.Equal(20, result.Min);
        Assert.Equal(20, result.Max);
    }

    [Fact]
    public void Calculate_MaximumWithNormalCritical_ReturnsHighestCriticalRoll()
    {
        var result = DamageCalculator.Calculate(CharmanderL10, BulbasaurL10, Ember, DamageRollPolicy.Maximum, CriticalHitPolicy.Normal);

        Assert.Equal(42, result.Max);
    }

    // Guaranteed: every hit is critical, so the range is the critical one (35..42 for case 2).
    [Fact]
    public void Calculate_GuaranteedCritical_UsesOnlyCriticalRolls()
    {
        var result = DamageCalculator.Calculate(CharmanderL10, BulbasaurL10, Ember, DamageRollPolicy.FullRange, CriticalHitPolicy.Guaranteed);

        Assert.Equal(35, result.Min);
        Assert.Equal(42, result.Max);
        Assert.Empty(result.Rolls);
        Assert.Equal(16, result.CriticalRolls.Count);
        Assert.Equal(1, result.CriticalHitChance);
    }

    // Normal: a critical is possible, so the range spans the lowest non-critical roll to the highest critical roll.
    [Fact]
    public void Calculate_NormalCritical_SpansNonCriticalMinToCriticalMax()
    {
        var result = DamageCalculator.Calculate(CharmanderL10, BulbasaurL10, Ember, DamageRollPolicy.FullRange, CriticalHitPolicy.Normal);

        Assert.Equal(17, result.Min);
        Assert.Equal(42, result.Max);
        Assert.Equal(1.0 / 16, result.CriticalHitChance);
    }

    [Fact]
    public void Calculate_EvaluateProbability_KeepsNonCriticalRangeAndReportsChance()
    {
        var result = DamageCalculator.Calculate(CharmanderL10, BulbasaurL10, Ember, DamageRollPolicy.FullRange, CriticalHitPolicy.EvaluateProbability);

        Assert.Equal(17, result.Min);
        Assert.Equal(20, result.Max);
        Assert.Equal(16, result.Rolls.Count);
        Assert.Equal(16, result.CriticalRolls.Count);
        Assert.Equal(1.0 / 16, result.CriticalHitChance);
    }

    [Fact]
    public void Calculate_HighCriticalRatioMove_HasOneInEightChance()
    {
        var highCritical = new Move(PokemonType.Fire, 40, highCriticalRatio: true);

        var result = DamageCalculator.Calculate(CharmanderL10, BulbasaurL10, highCritical, DamageRollPolicy.Minimum, CriticalHitPolicy.EvaluateProbability);

        Assert.Equal(1.0 / 8, result.CriticalHitChance);
    }

    // Case 2 non-critical rolls sum to 290 (17*5 + 18*5 + 19*5 + 20) -> 290/16 = 18.125.
    [Fact]
    public void Calculate_AverageWithoutCritical_IsMeanOfTheSixteenRolls()
    {
        var result = DamageCalculator.Calculate(CharmanderL10, BulbasaurL10, Ember, DamageRollPolicy.Average, CriticalHitPolicy.Never);

        Assert.Equal(290.0 / 16, result.Average);
        Assert.Equal(17, result.Min);
        Assert.Equal(20, result.Max);
        Assert.Empty(result.Rolls);
    }

    // Case 2 critical rolls sum to 614 -> 614/16 = 38.375.
    // Weighted: 15/16 * 18.125 + 1/16 * 38.375 = 16.9921875 + 2.3984375 = 19.390625.
    [Fact]
    public void Calculate_AverageWithCriticalProbability_IsWeightedByCriticalChance()
    {
        var result = DamageCalculator.Calculate(CharmanderL10, BulbasaurL10, Ember, DamageRollPolicy.Average, CriticalHitPolicy.EvaluateProbability);

        Assert.NotNull(result.Average);
        Assert.Equal(19.390625, result.Average.Value, precision: 10);
    }

    [Fact]
    public void Calculate_AverageWithGuaranteedCritical_IsMeanOfCriticalRolls()
    {
        var result = DamageCalculator.Calculate(CharmanderL10, BulbasaurL10, Ember, DamageRollPolicy.Average, CriticalHitPolicy.Guaranteed);

        Assert.Equal(614.0 / 16, result.Average);
    }

    [Fact]
    public void Calculate_Immune_DealsZeroDamage()
    {
        var ghost = new Combatant(20, Attack: 23, Defense: 21, SpecialAttack: 49, SpecialDefense: 23, PokemonType.Ghost, PokemonType.Poison);

        var result = DamageCalculator.Calculate(CharmanderL10, ghost, new Move(PokemonType.Normal, 40), DamageRollPolicy.FullRange, CriticalHitPolicy.Normal);

        Assert.Equal(0, result.Min);
        Assert.Equal(0, result.Max);
        Assert.All(result.Rolls, roll => Assert.Equal(0, roll));
        Assert.All(result.CriticalRolls, roll => Assert.Equal(0, roll));
        Assert.Equal(0, result.Effectiveness);
    }

    // Level 1, Atk 5, Power 10 vs Def 200: 5*10 = 50; *(2*1/5+2 = 2) = 100; /200 = 0; /50 = 0.
    // Physical floor -> 1; +2 = 3. Normal vs Rock/Steel (0.5x each): 3*5/10 = 1; 1*5/10 = 0 -> floor 1.
    // Random: 1*85/100 = 0 -> at least 1. Every roll is 1, never 0.
    [Fact]
    public void Calculate_DoubleResistedWeakHit_NeverDropsBelowOne()
    {
        var weak = new Combatant(1, Attack: 5, Defense: 5, SpecialAttack: 5, SpecialDefense: 5, PokemonType.Water);
        var wall = new Combatant(50, Attack: 50, Defense: 200, SpecialAttack: 50, SpecialDefense: 200, PokemonType.Rock, PokemonType.Steel);

        var result = DamageCalculator.Calculate(weak, wall, new Move(PokemonType.Normal, 10), DamageRollPolicy.FullRange, CriticalHitPolicy.Never);

        Assert.All(result.Rolls, roll => Assert.Equal(1, roll));
        Assert.Equal(0.25, result.Effectiveness);
    }

    // Same weak attacker, neutral matchup. Physical (Normal): base 0 -> floor 1 -> +2 = 3; rolls 2..3.
    // Special (Water): the special branch has no floor, so base 0 + 2 = 2; rolls 1..2.
    [Theory]
    [InlineData(PokemonType.Normal, 2, 3)]
    [InlineData(PokemonType.Water, 1, 2)]
    public void Calculate_OnlyPhysicalBaseDamageHasFloorOfOne(PokemonType moveType, int expectedMin, int expectedMax)
    {
        var weak = new Combatant(1, Attack: 5, Defense: 5, SpecialAttack: 5, SpecialDefense: 5, PokemonType.Grass);
        var wall = new Combatant(50, Attack: 50, Defense: 200, SpecialAttack: 50, SpecialDefense: 200, PokemonType.Fighting);

        var result = DamageCalculator.Calculate(weak, wall, new Move(moveType, 10), DamageRollPolicy.FullRange, CriticalHitPolicy.Never);

        Assert.Equal(expectedMin, result.Min);
        Assert.Equal(expectedMax, result.Max);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Move_NonPositivePower_Throws(int power)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Move(PokemonType.Normal, power));
    }

    [Fact]
    public void Calculate_UnknownPolicies_Throw()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DamageCalculator.Calculate(CharmanderL10, BulbasaurL10, Ember, (DamageRollPolicy)99, CriticalHitPolicy.Never));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DamageCalculator.Calculate(CharmanderL10, BulbasaurL10, Ember, DamageRollPolicy.Minimum, (CriticalHitPolicy)99));
    }

    private static readonly Combatant CharmanderL10 = new(10, Attack: 16, Defense: 16, SpecialAttack: 22, SpecialDefense: 18, PokemonType.Fire);
    private static readonly Combatant BulbasaurL10 = new(10, Attack: 16, Defense: 16, SpecialAttack: 19, SpecialDefense: 19, PokemonType.Grass, PokemonType.Poison);
    private static readonly Move Ember = new(PokemonType.Fire, 40);

    private static void AssertMatchesReference(Combatant attacker, Combatant defender, Move move, int[] expectedRolls, int[] expectedCriticalRolls)
    {
        var result = DamageCalculator.Calculate(attacker, defender, move, DamageRollPolicy.FullRange, CriticalHitPolicy.Normal);

        Assert.Equal(expectedRolls, result.Rolls);
        Assert.Equal(expectedCriticalRolls, result.CriticalRolls);
        Assert.Equal(expectedRolls[0], result.Min);
        Assert.Equal(expectedCriticalRolls[^1], result.Max);
    }
}