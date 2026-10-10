using PokeBattle.Core.Stats;

namespace PokeBattle.Core.Tests.Stats;

public class NatureEffectsTests
{
    public static readonly TheoryData<Nature> NeutralNatures = new(
        Nature.Hardy, Nature.Docile, Nature.Serious, Nature.Bashful, Nature.Quirky);

    [Theory]
    [MemberData(nameof(NeutralNatures))]
    public void GetMultiplier_NeutralNatures_NeverChangeAnyStat(Nature nature)
    {
        foreach (var statType in Enum.GetValues<StatType>())
        {
            var (numerator, denominator) = NatureEffects.GetMultiplier(nature, statType);

            Assert.Equal(numerator, denominator);
        }
    }

    [Fact]
    public void GetMultiplier_Adamant_BoostsAttackAndLowersSpecialAttack()
    {
        Assert.Equal((110, 100), NatureEffects.GetMultiplier(Nature.Adamant, StatType.Attack));
        Assert.Equal((90, 100), NatureEffects.GetMultiplier(Nature.Adamant, StatType.SpecialAttack));
        Assert.Equal((100, 100), NatureEffects.GetMultiplier(Nature.Adamant, StatType.Speed));
    }
}