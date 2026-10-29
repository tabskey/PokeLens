using PokeBattle.Application.Battles.ListTargets;

namespace PokeBattle.Application.Tests.Battles;

public class ListTargetsUseCaseTests
{
    private readonly ListTargetsUseCase useCase = new(PredictBattleOutcomeUseCaseTests.Package);

    [Fact]
    public void Execute_ListsWildTargetsInPokedexOrder()
    {
        // Eevee is a gift: not a target in V1. Sandshrew is LeafGreen only.
        Assert.Equal(["pidgey", "rattata", "ekans"], Execute("start").Select(t => t.Pokemon));
    }

    [Fact]
    public void Execute_TargetOnlyInFutureArea_IsNotListed()
    {
        Assert.DoesNotContain("spearow", Execute("start").Select(t => t.Pokemon));
        Assert.Contains("spearow", Execute("badge-1").Select(t => t.Pokemon));
    }

    [Fact]
    public void Execute_MethodBeforeItsMilestone_IsNotListed()
    {
        Assert.Equal(["walk"], Target("start", "rattata").Encounters.Select(e => e.Method));
    }

    [Fact]
    public void Execute_SameAreaWithTwoMethods_HasOneOptionPerMethodWithItsLevels()
    {
        Assert.Equal(
            [new TargetEncounter("route-a", "Route A", "old-rod", 10, 15, 20), new TargetEncounter("route-a", "Route A", "walk", 2, 5, 20)],
            Target("badge-1", "rattata").Encounters);
    }

    [Fact]
    public void Execute_PokemonInTwoUnlockedAreas_HasTwoOptionsInProgressionOrder()
    {
        Assert.Equal(["route-a", "route-b"], Target("badge-1", "pidgey").Encounters.Select(e => e.Area));
    }

    [Fact]
    public void Execute_SeveralSlots_SumTheirChance()
    {
        Assert.Equal(40, Assert.Single(Target("badge-1", "spearow").Encounters).Chance);
    }

    [Fact]
    public void Validate_UnknownVersionAndMilestone_ReportsBoth()
    {
        Assert.Equal(["milestone", "version"], useCase.Validate("ruby", null).Keys.Order());
        Assert.Throws<ArgumentException>(() => useCase.Execute("ruby", "start"));
    }

    private IReadOnlyList<TargetOption> Execute(string milestone) => useCase.Execute("firered", milestone);

    private TargetOption Target(string milestone, string pokemon) => Execute(milestone).Single(t => t.Pokemon == pokemon);
}
