using PokeBattle.Core.Battles;
using PokeBattle.Core.Capture;
using PokeBattle.Core.Damage;
using PokeBattle.Core.Types;

namespace PokeBattle.Core.Tests.Battles;

// Candidates against a wild Pidgey (2-5). Simulated numbers come from BattleSimulationServiceTests
// and CatchPlannerTests; here only the ordering is under test.
public class BattleRankingServiceTests
{
    private static readonly StatSpread RattataBase = new(30, 56, 35, 25, 35, 72);
    private static readonly BattlerSpec Pidgey = BattlerSpec.Wild(new(40, 45, 40, 35, 35, 56), PokemonType.Normal, PokemonType.Flying, new LevelRange(2, 5));
    private static readonly Move Tackle = new(PokemonType.Normal, 35);
    private static readonly Move Lick = new(PokemonType.Ghost, 20);

    // Strong: L16-20 Tackle (Favorable everywhere). Weak: L2-5 Tackle (Unfavorable when pessimistic).
    // Useless: only Lick, which Pidgey is immune to.
    private static readonly CandidateEvaluation Strong = Evaluate("strong", new LevelRange(16, 20), [Tackle]);
    private static readonly CandidateEvaluation Weak = Evaluate("weak", new LevelRange(2, 5), [Tackle]);
    private static readonly CandidateEvaluation Useless = Evaluate("useless", new LevelRange(16, 20), [Lick]);

    [Fact]
    public void Rank_Defeat_OrdersByMatchupFirst()
    {
        var ranked = BattleRankingService.Rank([Useless, Weak, Strong], BattleObjective.Defeat, ScenarioKind.Pessimistic);

        Assert.Equal(["strong", "weak", "useless"], ranked.Select(candidate => candidate.Name));
    }

    // Weak's pessimistic hit cannot KO (safe), Strong's can with a critical hit (risk 1/16), and
    // Useless cannot lower HP at all: safe HP reduction comes before capture chance.
    [Fact]
    public void Rank_Catch_PrefersSafeHpReduction()
    {
        var ranked = BattleRankingService.Rank([Useless, Strong, Weak], BattleObjective.Catch, ScenarioKind.Pessimistic);

        Assert.Equal(["weak", "strong", "useless"], ranked.Select(candidate => candidate.Name));
    }

    [Fact]
    public void Rank_TiesFallBackToName_AndRespectsTheLimit()
    {
        var twin = Strong with { Name = "another-strong" };

        var ranked = BattleRankingService.Rank([Strong, Weak, twin], BattleObjective.Defeat, ScenarioKind.Neutral, limit: 2);

        Assert.Equal(["another-strong", "strong"], ranked.Select(candidate => candidate.Name));
    }

    [Fact]
    public void Rank_InvalidLimit_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => BattleRankingService.Rank([Strong], BattleObjective.Defeat, ScenarioKind.Neutral, limit: 0));
    }

    private static CandidateEvaluation Evaluate(string name, LevelRange levels, IReadOnlyList<Move> moves)
    {
        var candidate = BattlerSpec.Wild(RattataBase, PokemonType.Normal, null, levels);
        return new CandidateEvaluation(
            name,
            BattleSimulationService.Simulate(candidate, moves, Pidgey, [Tackle], DamageRollPolicy.FullRange, CriticalHitPolicy.Never),
            [.. ScenarioCatalog.All.Select(scenario => CatchPlanner.Plan(candidate, moves, Pidgey, 255, scenario))]);
    }
}