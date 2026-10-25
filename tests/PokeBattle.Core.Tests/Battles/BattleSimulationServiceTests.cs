using PokeBattle.Core.Battles;
using PokeBattle.Core.Damage;
using PokeBattle.Core.Types;

namespace PokeBattle.Core.Tests.Battles;

// Base stats (Gen III): Rattata 30/56/35/25/35/72, Pidgey 40/45/40/35/35/56, Gastly 30/35/30/100/35/80.
// Stats follow ScenarioResolverTests (IV PROVISIONAL: Low 8, Average 15, High 23) and damage follows
// the pret/pokefirered order validated in DamageCalculatorTests. Policies: FullRange, no critical hits.
public class BattleSimulationServiceTests
{
    private static readonly BattlerSpec Rattata = BattlerSpec.Wild(new(30, 56, 35, 25, 35, 72), PokemonType.Normal, null, new LevelRange(2, 5));
    private static readonly BattlerSpec Pidgey = BattlerSpec.Wild(new(40, 45, 40, 35, 35, 56), PokemonType.Normal, PokemonType.Flying, new LevelRange(2, 5));
    private static readonly BattlerSpec RattataL16To20 = BattlerSpec.Wild(new(30, 56, 35, 25, 35, 72), PokemonType.Normal, null, new LevelRange(16, 20));
    private static readonly BattlerSpec Gastly = BattlerSpec.Wild(new(30, 35, 30, 100, 35, 80), PokemonType.Ghost, PokemonType.Poison, new LevelRange(13, 19));

    private static readonly Move Tackle = new(PokemonType.Normal, 35);
    private static readonly Move Bite = new(PokemonType.Dark, 60);
    private static readonly Move Lick = new(PokemonType.Ghost, 20);

    // Rattata (Tackle) vs Pidgey (Tackle), Route 1 (2-5), both wild.
    // Pessimistic: Rattata L2 Atk 6 vs Pidgey L5 (IV 23) Def 10, HP 20:
    //   6*35 = 210; *(2*2/5+2 = 2) = 420; /10 = 42; /50 = 0 -> physical floor 1; +2 = 3; STAB 4 -> 3..4.
    //   Turns ceil(20/4) = 5 .. ceil(20/3) = 7. Remaining HP 16..17.
    //   Pidgey L5 Atk 11 (+10%) vs Rattata L2 Def 6, HP 13: 11*35*4/6/50 = 5; +2 = 7; STAB 10 -> 8..10 -> 2..2 turns.
    // Neutral: both L3, IV 15: 5..6 damage each way, both 15 HP -> 3..3 vs 3..3 (tie).
    // Optimistic: mirror of pessimistic -> 8..10 vs 13 HP (2..2) against 3..4 vs 19 HP (5..7).
    [Fact]
    public void Simulate_RattataVsPidgey_GetsWorseFromOptimisticToPessimistic()
    {
        var results = BattleSimulationService.Simulate(Rattata, [Tackle], Pidgey, [Tackle], DamageRollPolicy.FullRange, CriticalHitPolicy.Never);

        Assert.Equal([ScenarioKind.Pessimistic, ScenarioKind.Neutral, ScenarioKind.Optimistic], results.Select(result => result.Scenario));

        var pessimistic = results[0];
        Assert.Equal(new TurnsToKo(5, 7), pessimistic.CandidateAttack!.TurnsToKo);
        Assert.Equal((3, 4), (pessimistic.CandidateAttack.Damage.Min, pessimistic.CandidateAttack.Damage.Max));
        Assert.Equal(20, pessimistic.CandidateAttack.TargetMaxHp);
        Assert.Equal(new HpRange(16, 17), pessimistic.CandidateAttack.RemainingHp);
        Assert.Equal(new TurnsToKo(2, 2), pessimistic.OpponentAttack!.TurnsToKo);
        Assert.Equal(KoClassification.FourOrMoreHits, pessimistic.CandidateKo);
        Assert.Equal(BattleOutcome.Unfavorable, pessimistic.Outcome);

        var neutral = results[1];
        Assert.Equal(new TurnsToKo(3, 3), neutral.CandidateAttack!.TurnsToKo);
        Assert.Equal(new TurnsToKo(3, 3), neutral.OpponentAttack!.TurnsToKo);
        Assert.Equal(new HpRange(9, 10), neutral.CandidateAttack.RemainingHp);
        Assert.Equal(KoClassification.ThreeHitKo, neutral.CandidateKo);
        Assert.Equal(BattleOutcome.Balanced, neutral.Outcome);

        var optimistic = results[2];
        Assert.Equal(new TurnsToKo(2, 2), optimistic.CandidateAttack!.TurnsToKo);
        Assert.Equal(new HpRange(3, 5), optimistic.CandidateAttack.RemainingHp);
        Assert.Equal(new TurnsToKo(5, 7), optimistic.OpponentAttack!.TurnsToKo);
        Assert.Equal(KoClassification.TwoHitKo, optimistic.CandidateKo);
        Assert.Equal(BattleOutcome.Favorable, optimistic.Outcome);
    }

    // Rattata (Tackle, Bite) vs Gastly (Lick). Tackle is Normal (Gastly is immune); Bite is Dark,
    // therefore special and 2x on Ghost. Lick is Ghost, which cannot hit a Normal type.
    // Pessimistic: Rattata L16 SpA ((50+8)*16/100 = 9) + 5 = 14 -> 14*90/100 = 12 (Hindering on Sp. Atk);
    //   Gastly L19 SpD ((70+23)*19/100 = 17) + 5 = 22, HP (60+23)*19/100 = 15 -> 44.
    //   12*60 = 720; *(2*16/5+2 = 8) = 5760; /22 = 261; /50 = 5; +2 = 7; Dark->Ghost 14 -> 11..14 -> 4..4 turns.
    // Neutral 18..22 vs 38 HP (2..3); optimistic 30..36 vs 31 HP (1..2).
    [Fact]
    public void Simulate_PicksTheMoveThatActuallyHits()
    {
        var results = BattleSimulationService.Simulate(RattataL16To20, [Tackle, Bite], Gastly, [Lick], DamageRollPolicy.FullRange, CriticalHitPolicy.Never);

        Assert.All(results, result => Assert.Equal(Bite, result.CandidateAttack!.Move));
        Assert.Equal([new TurnsToKo(4, 4), new TurnsToKo(2, 3), new TurnsToKo(1, 2)], results.Select(result => result.CandidateAttack!.TurnsToKo));
        Assert.Equal([KoClassification.FourOrMoreHits, KoClassification.TwoHitKo, KoClassification.PossibleKo], results.Select(result => result.CandidateKo));
        Assert.Equal((11, 14), (results[0].CandidateAttack!.Damage.Min, results[0].CandidateAttack!.Damage.Max));

        Assert.All(results, result => Assert.Null(result.OpponentAttack!.TurnsToKo));
        Assert.All(results, result => Assert.Equal(BattleOutcome.Favorable, result.Outcome));
    }

    // The Estimated Nature follows the chosen move's category: with Bite (special) the pessimistic
    // candidate has Sp. Atk 12 (14 hindered) while Attack stays neutral: ((112+8)*16/100 = 19) + 5 = 24.
    [Fact]
    public void Simulate_ResolvesTheCandidateForTheChosenMoveCategory()
    {
        var pessimistic = BattleSimulationService.SimulateScenario(
            RattataL16To20, [Tackle, Bite], Gastly, [Lick], ScenarioCatalog.Pessimistic, DamageRollPolicy.FullRange, CriticalHitPolicy.Never);

        Assert.Equal(12, pessimistic.Candidate.Combatant.SpecialAttack);
        Assert.Equal(24, pessimistic.Candidate.Combatant.Attack);
        Assert.Equal(16, pessimistic.Candidate.Level);
        Assert.Equal(19, pessimistic.Opponent.Level);
    }

    [Fact]
    public void Simulate_OnlyImmuneMove_IsNoDamageAndUnfavorable()
    {
        var results = BattleSimulationService.Simulate(RattataL16To20, [Tackle], Gastly, [Lick], DamageRollPolicy.FullRange, CriticalHitPolicy.Never);

        Assert.All(results, result =>
        {
            Assert.Equal(0, result.CandidateAttack!.Damage.Max);
            Assert.Null(result.CandidateAttack.TurnsToKo);
            Assert.Equal(KoClassification.NoDamage, result.CandidateKo);
            Assert.Equal(BattleOutcome.Unfavorable, result.Outcome);
        });
    }

    [Fact]
    public void Simulate_OpponentWithoutDamagingMoves_IsFavorable()
    {
        var results = BattleSimulationService.Simulate(Rattata, [Tackle], Pidgey, [], DamageRollPolicy.FullRange, CriticalHitPolicy.Never);

        Assert.All(results, result => Assert.Null(result.OpponentAttack));
        Assert.All(results, result => Assert.Equal(BattleOutcome.Favorable, result.Outcome));
        Assert.Equal(13, results[2].Opponent.MaxHp);
    }

    [Fact]
    public void Simulate_CandidateWithoutMoves_IsNoDamage()
    {
        var neutral = BattleSimulationService.SimulateScenario(
            Rattata, [], Pidgey, [Tackle], ScenarioCatalog.Neutral, DamageRollPolicy.FullRange, CriticalHitPolicy.Never);

        Assert.Null(neutral.CandidateAttack);
        Assert.Equal(KoClassification.NoDamage, neutral.CandidateKo);
        Assert.Equal(BattleOutcome.Unfavorable, neutral.Outcome);
        Assert.Equal(15, neutral.Candidate.MaxHp);
    }

    // Neutral Rattata L3 vs Pidgey L3 (15 HP), Normal moves with STAB:
    // power 35 -> 5..6 (3..3 turns), power 45 -> 5..7 (3..3 turns), power 60 -> 7..9 (2..3 turns).
    [Fact]
    public void Simulate_SameWorstCaseTurns_PrefersFewerBestCaseTurns()
    {
        var power60 = new Move(PokemonType.Normal, 60);

        var neutral = SimulateNeutral([Tackle, power60]);

        Assert.Equal(power60, neutral.CandidateAttack!.Move);
        Assert.Equal(new TurnsToKo(2, 3), neutral.CandidateAttack.TurnsToKo);
    }

    [Fact]
    public void Simulate_SameTurns_PrefersHigherMaximumDamage()
    {
        var power45 = new Move(PokemonType.Normal, 45);

        var neutral = SimulateNeutral([Tackle, power45]);

        Assert.Equal(power45, neutral.CandidateAttack!.Move);
        Assert.Equal(7, neutral.CandidateAttack.Damage.Max);
    }

    // Water 40 and Fire 40 are both special, non-STAB and neutral on Pidgey: identical 3..4 damage.
    [Fact]
    public void Simulate_FullTie_KeepsTheMoveListOrder()
    {
        var water = new Move(PokemonType.Water, 40);
        var fire = new Move(PokemonType.Fire, 40);

        Assert.Equal(water, SimulateNeutral([water, fire]).CandidateAttack!.Move);
        Assert.Equal(fire, SimulateNeutral([fire, water]).CandidateAttack!.Move);
    }

    [Fact]
    public void Simulate_MinimumRollPolicy_GivesSingleTurnCount()
    {
        var results = BattleSimulationService.Simulate(Rattata, [Tackle], Pidgey, [Tackle], DamageRollPolicy.Minimum, CriticalHitPolicy.Never);

        Assert.All(results, result => Assert.Equal(result.CandidateAttack!.TurnsToKo!.Min, result.CandidateAttack.TurnsToKo.Max));
        Assert.Equal(new TurnsToKo(7, 7), results[0].CandidateAttack!.TurnsToKo);
    }

    [Fact]
    public void SimulateScenario_NullArguments_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => BattleSimulationService.SimulateScenario(
            null!, [Tackle], Pidgey, [Tackle], ScenarioCatalog.Neutral, DamageRollPolicy.FullRange, CriticalHitPolicy.Never));
        Assert.Throws<ArgumentNullException>(() => BattleSimulationService.SimulateScenario(
            Rattata, null!, Pidgey, [Tackle], ScenarioCatalog.Neutral, DamageRollPolicy.FullRange, CriticalHitPolicy.Never));
        Assert.Throws<ArgumentNullException>(() => BattleSimulationService.SimulateScenario(
            Rattata, [Tackle], null!, [Tackle], ScenarioCatalog.Neutral, DamageRollPolicy.FullRange, CriticalHitPolicy.Never));
        Assert.Throws<ArgumentNullException>(() => BattleSimulationService.SimulateScenario(
            Rattata, [Tackle], Pidgey, null!, ScenarioCatalog.Neutral, DamageRollPolicy.FullRange, CriticalHitPolicy.Never));
        Assert.Throws<ArgumentNullException>(() => BattleSimulationService.SimulateScenario(
            Rattata, [Tackle], Pidgey, [Tackle], null!, DamageRollPolicy.FullRange, CriticalHitPolicy.Never));
    }

    private static ScenarioSimulation SimulateNeutral(IReadOnlyList<Move> candidateMoves)
        => BattleSimulationService.SimulateScenario(
            Rattata, candidateMoves, Pidgey, [Tackle], ScenarioCatalog.Neutral, DamageRollPolicy.FullRange, CriticalHitPolicy.Never);
}