using PokeBattle.Core.Battles;
using PokeBattle.Core.Capture;
using PokeBattle.Core.Damage;
using PokeBattle.Core.Types;

namespace PokeBattle.Core.Tests.Capture;

// Stats and damage as derived in BattleSimulationServiceTests (Pessimistic scenario). Pidgey catch
// rate 255, Poké Ball, no status: X = 255 * (3M - 2H) / 3M, Y = 1048560 / sqrt(sqrt(16711680 / X)).
public class CatchPlannerTests
{
    private static readonly BattlerSpec Rattata = BattlerSpec.Wild(new(30, 56, 35, 25, 35, 72), PokemonType.Normal, null, new LevelRange(2, 5));
    private static readonly BattlerSpec RattataL16To20 = BattlerSpec.Wild(new(30, 56, 35, 25, 35, 72), PokemonType.Normal, null, new LevelRange(16, 20));
    private static readonly BattlerSpec Pidgey = BattlerSpec.Wild(new(40, 45, 40, 35, 35, 56), PokemonType.Normal, PokemonType.Flying, new LevelRange(2, 5));

    private static readonly Move Tackle = new(PokemonType.Normal, 35);
    private static readonly Move Lick = new(PokemonType.Ghost, 20);

    // Rattata L2 Tackle vs Pidgey L5 (HP 20): 3..4, critical (3*2 = 6, STAB 9) 7..9 -> never KOs.
    // Weakest roll leaves 17 HP: X = 255 * (60 - 34) / 60 = 110; 16711680 / 110 = 151924;
    // sqrt = 389; sqrt = 19; Y = 1048560 / 19 = 55187.
    [Fact]
    public void Plan_HitThatCannotKo_IsSafeAndCapturesAtWeakestRollHp()
    {
        var plan = CatchPlanner.Plan(Rattata, [Tackle], Pidgey, 255, ScenarioCatalog.Pessimistic);

        Assert.Equal(ScenarioKind.Pessimistic, plan.Scenario);
        Assert.Equal(Tackle, plan.Hit!.Move);
        Assert.Equal(0, plan.KoRisk);
        Assert.True(plan.IsSafeHit);
        Assert.Equal(17, plan.TargetHp);
        Assert.Equal(110, plan.Capture[PokeBall.PokeBall].FinalCaptureRate);
        Assert.Equal(55187, plan.Capture[PokeBall.PokeBall].ShakeThreshold);
        Assert.Equal([PokeBall.PokeBall, PokeBall.GreatBall, PokeBall.UltraBall], plan.Capture.Keys);
    }

    // Rattata L16 (Atk 24 hindered -> 21) Tackle vs Pidgey L5 (Def 10, HP 20):
    // 21*35*8/10/50 = 11; +2 = 13; STAB 19 -> 16..19, no KO. Critical: 26, STAB 39 -> 33..39, always KO.
    // KO risk = 1/16 (the critical chance). Weakest roll leaves 4 HP: X = 255 * (60 - 8) / 60 = 221;
    // 16711680 / 221 = 75618; sqrt = 274; sqrt = 16; Y = 1048560 / 16 = 65535.
    [Fact]
    public void Plan_OnlyCriticalHitCanKo_ReportsRiskAndIsNotSafe()
    {
        var plan = CatchPlanner.Plan(RattataL16To20, [Tackle], Pidgey, 255, ScenarioCatalog.Pessimistic);

        Assert.Equal(1.0 / 16, plan.KoRisk);
        Assert.False(plan.IsSafeHit);
        Assert.Equal(4, plan.TargetHp);
        Assert.Equal(221, plan.Capture[PokeBall.PokeBall].FinalCaptureRate);
        Assert.Equal(65535, plan.Capture[PokeBall.PokeBall].ShakeThreshold);
    }

    // Same matchup with a power 10 Normal move: 21*10*8/10/50 = 3; +2 = 5; STAB 7 -> 5..7; critical
    // 10, STAB 15 -> 12..15, never KOs 20 HP. It beats the stronger Tackle (KO risk 1/16) and leaves 15 HP.
    [Fact]
    public void Plan_PrefersASafeMoveOverAStrongerRiskyOne()
    {
        var weak = new Move(PokemonType.Normal, 10);

        var plan = CatchPlanner.Plan(RattataL16To20, [Tackle, weak], Pidgey, 255, ScenarioCatalog.Pessimistic);

        Assert.Same(weak, plan.Hit!.Move);
        Assert.True(plan.IsSafeHit);
        Assert.Equal(15, plan.TargetHp);
    }

    // Lick is Ghost: Normal/Flying Pidgey is immune, so there is no hit and balls go at full HP (20):
    // X = 255 * (60 - 40) / 60 = 85.
    [Fact]
    public void Plan_NoDamagingMove_ThrowsAtFullHp()
    {
        var plan = CatchPlanner.Plan(Rattata, [Lick], Pidgey, 255, ScenarioCatalog.Pessimistic);

        Assert.Null(plan.Hit);
        Assert.False(plan.IsSafeHit);
        Assert.Equal(20, plan.TargetHp);
        Assert.Equal(85, plan.Capture[PokeBall.PokeBall].FinalCaptureRate);
    }
}