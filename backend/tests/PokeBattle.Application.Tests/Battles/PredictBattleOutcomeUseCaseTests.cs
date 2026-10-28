using PokeBattle.Application.Battles.PredictBattleOutcome;
using PokeBattle.Application.GameData;
using PokeBattle.Core.Battles;
using PokeBattle.Core.Types;

namespace PokeBattle.Application.Tests.Battles;

// A tiny hand-made package: route-a is open from the start, route-b and TM01 (Body Slam) only after
// the first badge; Ekans is FireRed only and Sandshrew LeafGreen only on route-a.
public class PredictBattleOutcomeUseCaseTests
{
    internal static readonly GamePackage Package = new(
        Species:
        [
            Species("pidgey", [PokemonType.Normal, PokemonType.Flying], new(40, 45, 40, 35, 35, 56), 255, Level("tackle")),
            Species("rattata", [PokemonType.Normal], new(30, 56, 35, 25, 35, 72), 255, Level("tackle"), new LearnsetEntry("body-slam", LearnMethod.Machine, 0)),
            Species("ekans", [PokemonType.Poison], new(35, 60, 44, 40, 54, 55), 255, Level("tackle")),
            Species("sandshrew", [PokemonType.Ground], new(50, 75, 85, 20, 30, 40), 255, Level("tackle")),
            Species("spearow", [PokemonType.Normal, PokemonType.Flying], new(40, 60, 30, 31, 31, 70), 255, Level("tackle")),
        ],
        Moves:
        [
            new MoveData(1, "tackle", PokemonType.Normal, MoveCategory.Physical, 35, 95, 35, false, null),
            new MoveData(2, "body-slam", PokemonType.Normal, MoveCategory.Physical, 85, 100, 15, false, "tm01"),
        ],
        Encounters:
        [
            Walk("route-a", "firered", "pidgey"),
            Walk("route-a", "firered", "rattata"),
            Walk("route-a", "firered", "ekans"),
            Walk("route-a", "leafgreen", "pidgey"),
            Walk("route-a", "leafgreen", "rattata"),
            Walk("route-a", "leafgreen", "sandshrew"),
            Walk("route-b", "firered", "spearow"),
        ],
        Progression: new ProgressionData(
            ["start", "badge-1"],
            [new Unlock("walk", "start")],
            [new Unlock("route-a", "start"), new Unlock("route-b", "badge-1")],
            [new Unlock("tm01", "badge-1")]));

    private readonly PredictBattleOutcomeUseCase useCase = new(Package);

    [Fact]
    public void Execute_PokemonAvailableOnlyLater_IsNotRecommended()
    {
        Assert.DoesNotContain("spearow", Candidates(Request(milestone: "start")));
        Assert.Contains("spearow", Candidates(Request(milestone: "badge-1")));
    }

    [Fact]
    public void Execute_MachineNotObtained_MoveIsLeftOut()
    {
        Assert.All(Rattata(Request(milestone: "start")).Scenarios, scenario => Assert.Equal("tackle", scenario.CandidateAttack!.Move));
        Assert.All(Rattata(Request(milestone: "badge-1")).Scenarios, scenario => Assert.Equal("body-slam", scenario.CandidateAttack!.Move));
    }

    [Fact]
    public void Execute_LeafGreenExclusive_IsNotRecommendedInFireRed()
    {
        Assert.DoesNotContain("sandshrew", Candidates(Request(version: "firered")));
        Assert.Contains("sandshrew", Candidates(Request(version: "leafgreen")));
    }

    [Fact]
    public void Execute_Catch_ReturnsCaptureOddsForEveryScenario()
    {
        var response = useCase.Execute(Request() with { Objective = BattleObjective.Catch, Scenario = ScenarioKind.Neutral });

        Assert.Equal(ScenarioKind.Neutral, response.RankedBy);
        Assert.Equal((2, 5), (response.TargetMinLevel, response.TargetMaxLevel));
        Assert.Equal([PokemonType.Normal, PokemonType.Flying], response.TargetTypes);
        Assert.Equal([PokemonType.Normal], response.Candidates.Single(c => c.Pokemon == "rattata").Types);
        Assert.All(response.Candidates, candidate =>
        {
            Assert.Equal(3, candidate.Scenarios.Count);
            Assert.All(candidate.Scenarios, scenario => Assert.NotNull(scenario.Catch));
        });
    }

    [Fact]
    public void Validate_ReportsEachInvalidField()
    {
        var errors = useCase.Validate(new PredictBattleOutcomeRequest("mew", "route-a", "ruby", "earth-badge", (BattleObjective)9, (ScenarioKind)9));

        Assert.Equal(["milestone", "objective", "scenario", "target", "version"], errors.Keys.Order());
    }

    [Fact]
    public void Validate_TargetNotInArea_ReportsArea()
    {
        var errors = useCase.Validate(Request() with { Area = "route-b" });

        Assert.Equal(["area"], errors.Keys);
        Assert.Throws<ArgumentException>(() => useCase.Execute(Request() with { Area = "route-b" }));
    }

    private static PredictBattleOutcomeRequest Request(string version = "firered", string milestone = "start")
        => new("pidgey", "route-a", version, milestone, BattleObjective.Defeat, ScenarioKind.Pessimistic);

    private List<string> Candidates(PredictBattleOutcomeRequest request)
        => [.. useCase.Execute(request).Candidates.Select(candidate => candidate.Pokemon)];

    private CandidateResult Rattata(PredictBattleOutcomeRequest request)
        => useCase.Execute(request).Candidates.Single(candidate => candidate.Pokemon == "rattata");

    private static SpeciesData Species(string name, IReadOnlyList<PokemonType> types, StatSpread baseStats, int captureRate, params LearnsetEntry[] learnset)
        => new(0, name, types, baseStats, [], captureRate, learnset);

    private static LearnsetEntry Level(string move) => new(move, LearnMethod.LevelUp, 1);

    private static EncounterData Walk(string area, string version, string pokemon)
        => new(area, area, version, pokemon, "walk", 2, 5, 20, []);
}