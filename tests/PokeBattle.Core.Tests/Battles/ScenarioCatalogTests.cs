using PokeBattle.Core.Battles;
using PokeBattle.Core.Damage;

namespace PokeBattle.Core.Tests.Battles;

public class ScenarioCatalogTests
{
    [Fact]
    public void Pessimistic_PutsBothSidesAgainstTheCandidate()
    {
        var scenario = ScenarioCatalog.Pessimistic;

        Assert.Equal(new SideProfile(NatureProfile.Hindering, IvProfile.Low, EvProfile.None, LevelProfile.Lowest), scenario.Candidate);
        Assert.Equal(new SideProfile(NatureProfile.Favoring, IvProfile.High, EvProfile.None, LevelProfile.Highest), scenario.Opponent);
    }

    [Fact]
    public void Neutral_IsTheMiddleForBothSides()
    {
        var middle = new SideProfile(NatureProfile.Neutral, IvProfile.Average, EvProfile.None, LevelProfile.Midpoint);

        Assert.Equal(middle, ScenarioCatalog.Neutral.Candidate);
        Assert.Equal(middle, ScenarioCatalog.Neutral.Opponent);
    }

    [Fact]
    public void Optimistic_MirrorsPessimistic()
    {
        Assert.Equal(ScenarioCatalog.Pessimistic.Opponent, ScenarioCatalog.Optimistic.Candidate);
        Assert.Equal(ScenarioCatalog.Pessimistic.Candidate, ScenarioCatalog.Optimistic.Opponent);
    }

    [Fact]
    public void All_HasTheThreeScenariosInOrder()
    {
        Assert.Equal(
            [ScenarioKind.Pessimistic, ScenarioKind.Neutral, ScenarioKind.Optimistic],
            ScenarioCatalog.All.Select(scenario => scenario.Kind));
    }

    [Fact]
    public void BattleScenario_CarriesNoRngPolicy()
    {
        var propertyTypes = typeof(BattleScenario).GetProperties().Select(property => property.PropertyType).ToList();

        Assert.DoesNotContain(typeof(DamageRollPolicy), propertyTypes);
        Assert.DoesNotContain(typeof(CriticalHitPolicy), propertyTypes);
    }

    // PROVISIONAL values, pending review in docs/decisoes-cenarios.md.
    [Theory]
    [InlineData(IvProfile.Low, 8)]
    [InlineData(IvProfile.Average, 15)]
    [InlineData(IvProfile.High, 23)]
    [InlineData(IvProfile.Maximum, 31)]
    public void IndividualValueFor_MatchesDocumentedProfiles(IvProfile profile, int expected)
    {
        Assert.Equal(expected, ScenarioCatalog.IndividualValueFor(profile));
    }

    [Fact]
    public void EffortValueFor_WildPokemonHaveNoEvs()
    {
        Assert.Equal(0, ScenarioCatalog.EffortValueFor(EvProfile.None));
    }

    [Fact]
    public void ProfileValues_UnknownProfile_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ScenarioCatalog.IndividualValueFor((IvProfile)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => ScenarioCatalog.EffortValueFor((EvProfile)99));
    }
}