using PokeBattle.Application.Auditing;
using PokeBattle.Application.Battles.PredictBattleOutcome;
using PokeBattle.Application.GameData;
using PokeBattle.Core.Battles;
using PokeBattle.Core.Types;

namespace PokeBattle.Application.Tests.Battles;

// A tiny hand-made package: route-a is open from the start, route-b, the Old Rod and TM01 (Body Slam)
// only after the first badge; Ekans is FireRed only and Sandshrew LeafGreen only on route-a. Rattata
// is also fished on route-a at higher levels, Pidgey also walks on route-b and Eevee is a gift.
public class PredictBattleOutcomeUseCaseTests
{
    internal static readonly GamePackage Package = new(
        Species:
        [
            Species(16, "pidgey", [PokemonType.Normal, PokemonType.Flying], new(40, 45, 40, 35, 35, 56), 255, Level("tackle")),
            Species(19, "rattata", [PokemonType.Normal], new(30, 56, 35, 25, 35, 72), 255, Level("tackle"), new LearnsetEntry("body-slam", LearnMethod.Machine, 0)),
            Species(23, "ekans", [PokemonType.Poison], new(35, 60, 44, 40, 54, 55), 255, Level("tackle")),
            Species(27, "sandshrew", [PokemonType.Ground], new(50, 75, 85, 20, 30, 40), 255, Level("tackle")),
            Species(21, "spearow", [PokemonType.Normal, PokemonType.Flying], new(40, 60, 30, 31, 31, 70), 255, Level("tackle")),
            Species(133, "eevee", [PokemonType.Normal], new(55, 55, 50, 45, 65, 55), 45, Level("tackle")),
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
            Walk("route-b", "firered", "spearow"),
            Walk("route-b", "firered", "pidgey"),
            Encounter("route-a", "firered", "rattata", "old-rod", 10, 15),
            Encounter("route-a", "firered", "eevee", "gift", 25, 25),
        ],
        Progression: new ProgressionData(
            ["start", "badge-1"],
            [new Unlock("walk", "start"), new Unlock("gift", "start"), new Unlock("old-rod", "badge-1")],
            [new AreaUnlock("route-a", "start", "Route A"), new AreaUnlock("route-b", "badge-1", "Route B")],
            [new Unlock("tm01", "badge-1")]));

    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    private readonly RecordingAuditLog auditLog = new();
    private readonly PredictBattleOutcomeUseCase useCase;

    public PredictBattleOutcomeUseCaseTests()
    {
        useCase = new(Package, auditLog, new FixedClock(Now));
    }

    [Fact]
    public async Task ExecuteAsync_RecordsRequestAndCompletionWithUserAndCandidates()
    {
        var response = await useCase.ExecuteAsync(Request(), "user-1", CancellationToken.None);

        var candidates = response.Candidates.Select(c => c.Pokemon).ToList();
        Assert.Equal(
            [
                new AuditEntry(AuditEvents.BattlePredictionRequested, "user-1", "pidgey", "route-a", "walk", "firered", "start", BattleObjective.Defeat, [], Now),
                new AuditEntry(AuditEvents.BattlePredictionCompleted, "user-1", "pidgey", "route-a", "walk", "firered", "start", BattleObjective.Defeat, candidates, Now),
            ],
            auditLog.Entries,
            AuditComparer);
        Assert.NotEmpty(candidates);
    }

    [Fact]
    public async Task ExecuteAsync_Anonymous_RecordsNoUser()
    {
        await useCase.ExecuteAsync(Request(), null, CancellationToken.None);

        Assert.All(auditLog.Entries, entry => Assert.Null(entry.UserId));
    }

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
        var errors = useCase.Validate(new PredictBattleOutcomeRequest("mew", "route-a", "walk", "ruby", "earth-badge", (BattleObjective)9, (ScenarioKind)9));

        Assert.Equal(["milestone", "objective", "scenario", "target", "version"], errors.Keys.Order());
    }

    [Fact]
    public void Validate_TargetNotInArea_ReportsArea()
    {
        var errors = useCase.Validate(Request() with { Area = "route-b" });

        Assert.Equal(["area"], errors.Keys);
        Assert.Throws<ArgumentException>(() => useCase.Execute(Request() with { Area = "route-b" }));
    }

    [Fact]
    public void Validate_MethodNotUnlocked_ReportsMethod()
    {
        var errors = useCase.Validate(Request() with { Target = "rattata", Method = "old-rod" });

        Assert.Equal(["method"], errors.Keys);
    }

    [Fact]
    public void Validate_GiftIsNotAWildMethod_ReportsMethod()
    {
        Assert.Equal(["method"], useCase.Validate(Request() with { Target = "eevee", Method = "gift" }).Keys);
    }

    [Theory]
    [InlineData("walk", 2, 5)]
    [InlineData("old-rod", 10, 15)]
    public void Execute_TargetLevels_ComeFromTheChosenAreaAndMethod(string method, int min, int max)
    {
        var response = useCase.Execute(Request(milestone: "badge-1") with { Target = "rattata", Method = method });

        Assert.Equal((min, max), (response.TargetMinLevel, response.TargetMaxLevel));
    }

    private static PredictBattleOutcomeRequest Request(string version = "firered", string milestone = "start")
        => new("pidgey", "route-a", "walk", version, milestone, BattleObjective.Defeat, ScenarioKind.Pessimistic);

    private List<string> Candidates(PredictBattleOutcomeRequest request)
        => [.. useCase.Execute(request).Candidates.Select(candidate => candidate.Pokemon)];

    private CandidateResult Rattata(PredictBattleOutcomeRequest request)
        => useCase.Execute(request).Candidates.Single(candidate => candidate.Pokemon == "rattata");

    private static SpeciesData Species(int id, string name, IReadOnlyList<PokemonType> types, StatSpread baseStats, int captureRate, params LearnsetEntry[] learnset)
        => new(id, name, types, baseStats, [], captureRate, learnset);

    private static LearnsetEntry Level(string move) => new(move, LearnMethod.LevelUp, 1);

    private static readonly IEqualityComparer<AuditEntry> AuditComparer = EqualityComparer<AuditEntry>.Create(
        (x, y) => x! with { Candidates = [] } == y! with { Candidates = [] } && x.Candidates.SequenceEqual(y.Candidates));

    private sealed class RecordingAuditLog : IAuditLog
    {
        public List<AuditEntry> Entries { get; } = [];

        public Task RecordAsync(AuditEntry entry, CancellationToken cancellationToken)
        {
            Entries.Add(entry);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<AuditLogItem>> LatestAsync(int count, CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private static EncounterData Walk(string area, string version, string pokemon)
        => Encounter(area, version, pokemon, "walk", 2, 5);

    private static EncounterData Encounter(string area, string version, string pokemon, string method, int minLevel, int maxLevel)
        => new(area, area, version, pokemon, method, minLevel, maxLevel, 20, []);
}