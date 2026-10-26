using PokeBattle.Application.GameData;
using PokeBattle.Application.Tests.Battles;

namespace PokeBattle.Application.Tests.GameData;

public class GameOptionsQueryTests
{
    private readonly GameOptionsQuery query = new(PredictBattleOutcomeUseCaseTests.Package);

    [Fact]
    public void Execute_ListsAreasOfTheVersionWithTheirMilestone()
    {
        var options = query.Execute("firered");

        Assert.Equal(["start", "badge-1"], options.Milestones);
        Assert.Equal(
            [new AreaOption("route-a", "start", ["ekans", "pidgey", "rattata"]), new AreaOption("route-b", "badge-1", ["spearow"])],
            options.Areas,
            AreaComparer);
    }

    [Fact]
    public void Execute_OtherVersion_HasItsOwnPokemon()
    {
        var area = Assert.Single(query.Execute("leafgreen").Areas);

        Assert.Equal(["pidgey", "rattata", "sandshrew"], area.Pokemon);
    }

    private static readonly IEqualityComparer<AreaOption> AreaComparer = EqualityComparer<AreaOption>.Create(
        (x, y) => x!.Area == y!.Area && x.Milestone == y.Milestone && x.Pokemon.SequenceEqual(y.Pokemon));
}