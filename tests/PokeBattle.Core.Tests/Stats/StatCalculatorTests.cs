using PokeBattle.Core.Stats;

namespace PokeBattle.Core.Tests.Stats;

public class StatCalculatorTests
{
    // Case 1 — Base 100, IV 31, EV 252, Level 100.
    // floor(EV/4)   = floor(252/4) = 63
    // preLevel      = 2*100 + 31 + 63 = 294
    // levelScaled   = floor(294*100/100) = 294
    // HP            = 294 + 100 + 10 = 404
    // preNature     = 294 + 5 = 299
    // Adamant (+Atk):               floor(299*110/100) = floor(328.9) = 328
    // Modest (-Atk, since Modest raises SpAtk and lowers Atk): floor(299*90/100) = floor(269.1) = 269
    // Hardy (neutral):              299
    [Fact]
    public void CalculateHp_Case1_MatchesManualCalculation()
    {
        var hp = StatCalculator.CalculateHp(baseStat: 100, individualValue: 31, effortValue: 252, level: 100);

        Assert.Equal(404, hp);
    }

    [Theory]
    [InlineData(Nature.Adamant, 328)]
    [InlineData(Nature.Modest, 269)]
    [InlineData(Nature.Hardy, 299)]
    public void CalculateStat_Case1_MatchesManualCalculation(Nature nature, int expected)
    {
        var attack = StatCalculator.CalculateStat(
            baseStat: 100, individualValue: 31, effortValue: 252, level: 100, statType: StatType.Attack, nature: nature);

        Assert.Equal(expected, attack);
    }

    // Case 2 — Base 50, IV 0, EV 0, Level 50.
    // floor(EV/4)  = 0
    // preLevel     = 2*50 + 0 + 0 = 100
    // levelScaled  = floor(100*50/100) = 50
    // HP           = 50 + 50 + 10 = 110
    // preNature    = 50 + 5 = 55
    // Adamant: floor(55*110/100) = floor(60.5) = 60
    // Modest:  floor(55*90/100)  = floor(49.5) = 49
    // Hardy:   55
    [Fact]
    public void CalculateHp_Case2_MatchesManualCalculation()
    {
        var hp = StatCalculator.CalculateHp(baseStat: 50, individualValue: 0, effortValue: 0, level: 50);

        Assert.Equal(110, hp);
    }

    [Theory]
    [InlineData(Nature.Adamant, 60)]
    [InlineData(Nature.Modest, 49)]
    [InlineData(Nature.Hardy, 55)]
    public void CalculateStat_Case2_MatchesManualCalculation(Nature nature, int expected)
    {
        var attack = StatCalculator.CalculateStat(
            baseStat: 50, individualValue: 0, effortValue: 0, level: 50, statType: StatType.Attack, nature: nature);

        Assert.Equal(expected, attack);
    }

    // Case 3 — Base 120, IV 15, EV 85, Level 77 (forces truncation both in floor(EV/4) and in the
    // level-scaling step, unlike cases 1 and 2 where every intermediate division was already exact).
    // floor(EV/4)  = floor(85/4) = 21
    // preLevel     = 2*120 + 15 + 21 = 276
    // levelScaled  = floor(276*77/100) = floor(212.52) = 212
    // HP           = 212 + 77 + 10 = 299
    // preNature    = 212 + 5 = 217
    // Adamant: floor(217*110/100) = floor(238.7) = 238
    // Modest:  floor(217*90/100)  = floor(195.3) = 195
    // Hardy:   217
    [Fact]
    public void CalculateHp_Case3_MatchesManualCalculation()
    {
        var hp = StatCalculator.CalculateHp(baseStat: 120, individualValue: 15, effortValue: 85, level: 77);

        Assert.Equal(299, hp);
    }

    [Theory]
    [InlineData(Nature.Adamant, 238)]
    [InlineData(Nature.Modest, 195)]
    [InlineData(Nature.Hardy, 217)]
    public void CalculateStat_Case3_MatchesManualCalculation(Nature nature, int expected)
    {
        var attack = StatCalculator.CalculateStat(
            baseStat: 120, individualValue: 15, effortValue: 85, level: 77, statType: StatType.Attack, nature: nature);

        Assert.Equal(expected, attack);
    }
}