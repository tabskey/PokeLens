using PokeBattle.Core.Battles;
using PokeBattle.Core.Stats;
using PokeBattle.Core.Types;

namespace PokeBattle.Core.Tests.Battles;

// Base stats (Generation III): Rattata 30/56/35/25/35/72, Pidgey 40/45/40/35/35/56.
// Route 1 levels: 2-5. Formulas as in StatCalculatorTests; EV 0 everywhere, so floor(EV/4) = 0.
// IV values come from ScenarioCatalog (PROVISIONAL: Low 8, Average 15, High 23).
public class ScenarioResolverTests
{
    private static readonly StatSpread RattataBase = new(30, 56, 35, 25, 35, 72);
    private static readonly StatSpread PidgeyBase = new(40, 45, 40, 35, 35, 56);
    private static readonly LevelRange Route1 = new(2, 5);

    // Wild Rattata as the candidate, attacking with a physical move:
    // Pessimistic  L2, IV 8,  Atk -10%: HP (60+8)*2/100 = 1 -> 1+2+10 = 13;  Atk ((112+8)*2/100 = 2) + 5 = 7 -> 7*90/100 = 6
    // Neutral      L3, IV 15, neutral:  HP (60+15)*3/100 = 2 -> 2+3+10 = 15; Atk ((112+15)*3/100 = 3) + 5 = 8
    // Optimistic   L5, IV 23, Atk +10%: HP (60+23)*5/100 = 4 -> 4+5+10 = 19; Atk ((112+23)*5/100 = 6) + 5 = 11 -> 11*110/100 = 12
    [Fact]
    public void Resolve_EstimatedCandidate_DiffersAcrossScenarios()
    {
        var rattata = BattlerSpec.Wild(RattataBase, PokemonType.Normal, null, Route1);

        var resolved = ScenarioCatalog.All
            .Select(scenario => ScenarioResolver.Resolve(rattata, scenario, BattleSide.Candidate, MoveCategory.Physical))
            .ToList();

        Assert.Equal([2, 3, 5], resolved.Select(battler => battler.Level));
        Assert.Equal([13, 15, 19], resolved.Select(battler => battler.MaxHp));
        Assert.Equal([6, 8, 12], resolved.Select(battler => battler.Combatant.Attack));
    }

    // Wild Pidgey as the opponent goes the other way (pessimistic = strongest opponent):
    // Pessimistic  L5, IV 23, Atk +10%: HP (80+23)*5/100 = 5 -> 20; Atk ((90+23)*5/100 = 5) + 5 = 10 -> 11
    // Neutral      L3, IV 15:           HP (80+15)*3/100 = 2 -> 15; Atk ((90+15)*3/100 = 3) + 5 = 8
    // Optimistic   L2, IV 8,  Atk -10%: HP (80+8)*2/100 = 1 -> 13;  Atk ((90+8)*2/100 = 1) + 5 = 6 -> 5
    [Fact]
    public void Resolve_EstimatedOpponent_MovesInTheOppositeDirection()
    {
        var pidgey = BattlerSpec.Wild(PidgeyBase, PokemonType.Normal, PokemonType.Flying, Route1);

        var resolved = ScenarioCatalog.All
            .Select(scenario => ScenarioResolver.Resolve(pidgey, scenario, BattleSide.Opponent, MoveCategory.Physical))
            .ToList();

        Assert.Equal([5, 3, 2], resolved.Select(battler => battler.Level));
        Assert.Equal([20, 15, 13], resolved.Select(battler => battler.MaxHp));
        Assert.Equal([11, 8, 5], resolved.Select(battler => battler.Combatant.Attack));
        Assert.All(resolved, battler => Assert.Equal(PokemonType.Flying, battler.Combatant.Type2));
    }

    // Known Rattata: L4, Adamant (+Atk, -SpA), IV 31, EV 0.
    // HP  (60+31)*4/100 = 3 -> 3+4+10 = 17
    // Atk (112+31)*4/100 = 5 -> 10 -> 10*110/100 = 11;  Def (70+31)*4/100 = 4 -> 9
    // SpA (50+31)*4/100 = 3 -> 8 -> 8*90/100 = 7;        SpD (70+31)*4/100 = 4 -> 9
    [Theory]
    [InlineData(DataSource.Known)]
    [InlineData(DataSource.UserProvided)]
    public void Resolve_FixedAttributes_AreIdenticalAcrossScenarios(DataSource source)
    {
        var rattata = new BattlerSpec(
            RattataBase, PokemonType.Normal, null,
            LevelRange.Exactly(4), source,
            Nature.Adamant, source,
            StatSpread.Uniform(31), source,
            StatSpread.Uniform(0), source);

        var resolved = ScenarioCatalog.All
            .Select(scenario => ScenarioResolver.Resolve(rattata, scenario, BattleSide.Candidate, MoveCategory.Physical))
            .ToList();

        Assert.All(resolved, battler => Assert.Equal(resolved[0], battler));
        Assert.Equal(new ResolvedBattler(4, 17, new(4, Attack: 11, Defense: 9, SpecialAttack: 7, SpecialDefense: 9, PokemonType.Normal)), resolved[0]);
    }

    // Only the Nature is Estimated: Rattata L50, IV 15, EV 0.
    // Atk pre-Nature: (112+15)*50/100 = 63 -> 68. Hindering 68*90/100 = 61; Favoring 68*110/100 = 74.
    // SpA pre-Nature: (50+15)*50/100 = 32 -> 37 (untouched by a physical move's profile).
    [Fact]
    public void Resolve_EstimatedNatureOnly_AffectsOnlyTheOffensiveStat()
    {
        var rattata = KnownExcept(nature: false);

        var resolved = ScenarioCatalog.All
            .Select(scenario => ScenarioResolver.Resolve(rattata, scenario, BattleSide.Candidate, MoveCategory.Physical))
            .ToList();

        Assert.Equal([61, 68, 74], resolved.Select(battler => battler.Combatant.Attack));
        Assert.All(resolved, battler => Assert.Equal(37, battler.Combatant.SpecialAttack));
        Assert.All(resolved, battler => Assert.Equal(resolved[0].Combatant.Defense, battler.Combatant.Defense));
        Assert.All(resolved, battler => Assert.Equal(resolved[0].Combatant.SpecialDefense, battler.Combatant.SpecialDefense));
        Assert.All(resolved, battler => Assert.Equal(resolved[0].MaxHp, battler.MaxHp));
    }

    // Same, attacking with a special move: SpA 37 -> Hindering 33, Favoring 40; Attack stays 68.
    [Fact]
    public void Resolve_EstimatedNatureWithSpecialMove_AffectsSpecialAttack()
    {
        var rattata = KnownExcept(nature: false);

        var resolved = ScenarioCatalog.All
            .Select(scenario => ScenarioResolver.Resolve(rattata, scenario, BattleSide.Candidate, MoveCategory.Special))
            .ToList();

        Assert.Equal([33, 37, 40], resolved.Select(battler => battler.Combatant.SpecialAttack));
        Assert.All(resolved, battler => Assert.Equal(68, battler.Combatant.Attack));
    }

    // Only the IVs are Estimated: Rattata L50, Hardy, EV 0.
    // IV 8:  Atk (112+8)*50/100 = 60 -> 65;  HP (60+8)*50/100 = 34 -> 34+50+10 = 94
    // IV 15: Atk (112+15)*50/100 = 63 -> 68; HP (60+15)*50/100 = 37 -> 97
    // IV 23: Atk (112+23)*50/100 = 67 -> 72; HP (60+23)*50/100 = 41 -> 101
    [Fact]
    public void Resolve_EstimatedIvsOnly_UseTheScenarioIvProfile()
    {
        var rattata = KnownExcept(ivs: false);

        var resolved = ScenarioCatalog.All
            .Select(scenario => ScenarioResolver.Resolve(rattata, scenario, BattleSide.Candidate, MoveCategory.Physical))
            .ToList();

        Assert.Equal([65, 68, 72], resolved.Select(battler => battler.Combatant.Attack));
        Assert.Equal([94, 97, 101], resolved.Select(battler => battler.MaxHp));
    }

    // Only the level is Estimated, range 10-20 (midpoint (10+20)/2 = 15): Rattata Hardy, IV 15, EV 0.
    // L10: Atk (127*10/100 = 12) + 5 = 17; HP 75*10/100 = 7 -> 7+10+10 = 27
    // L15: Atk (127*15/100 = 19) + 5 = 24; HP 75*15/100 = 11 -> 36
    // L20: Atk (127*20/100 = 25) + 5 = 30; HP 75*20/100 = 15 -> 45
    [Fact]
    public void Resolve_EstimatedLevelOnly_PicksLowestMidpointHighest()
    {
        var rattata = KnownExcept(level: false, levelRange: new LevelRange(10, 20));

        var resolved = ScenarioCatalog.All
            .Select(scenario => ScenarioResolver.Resolve(rattata, scenario, BattleSide.Candidate, MoveCategory.Physical))
            .ToList();

        Assert.Equal([10, 15, 20], resolved.Select(battler => battler.Level));
        Assert.Equal([17, 24, 30], resolved.Select(battler => battler.Combatant.Attack));
        Assert.Equal([27, 36, 45], resolved.Select(battler => battler.MaxHp));
    }

    // Route 1 (2-5): floor((2+5)/2) = 3, not 3.5 rounded to 4.
    [Fact]
    public void Resolve_MidpointLevel_IsTruncated()
    {
        var resolved = ScenarioResolver.Resolve(
            BattlerSpec.Wild(RattataBase, PokemonType.Normal, null, Route1), ScenarioCatalog.Neutral, BattleSide.Candidate, MoveCategory.Physical);

        Assert.Equal(3, resolved.Level);
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(5, 101)]
    [InlineData(6, 5)]
    public void LevelRange_Invalid_Throws(int min, int max)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LevelRange(min, max));
    }

    [Fact]
    public void BattlerSpec_KnownLevelRange_Throws()
    {
        Assert.Throws<ArgumentException>(() => new BattlerSpec(
            RattataBase, PokemonType.Normal, null, Route1, DataSource.Known,
            null, DataSource.Estimated, null, DataSource.Estimated, null, DataSource.Estimated));
    }

    [Fact]
    public void BattlerSpec_EstimatedAttributeWithValue_Throws()
    {
        Assert.Throws<ArgumentException>(() => new BattlerSpec(
            RattataBase, PokemonType.Normal, null, Route1, DataSource.Estimated,
            Nature.Adamant, DataSource.Estimated, null, DataSource.Estimated, null, DataSource.Estimated));
    }

    [Fact]
    public void BattlerSpec_KnownAttributeWithoutValue_Throws()
    {
        Assert.Throws<ArgumentException>(() => new BattlerSpec(
            RattataBase, PokemonType.Normal, null, Route1, DataSource.Estimated,
            null, DataSource.Estimated, null, DataSource.Known, null, DataSource.Estimated));
    }

    /// <summary>Rattata L50, Hardy, IV 15, EV 0, every attribute Known except the ones set to false.</summary>
    private static BattlerSpec KnownExcept(bool level = true, bool nature = true, bool ivs = true, LevelRange? levelRange = null)
        => new(
            RattataBase, PokemonType.Normal, null,
            level ? LevelRange.Exactly(50) : levelRange ?? Route1, level ? DataSource.Known : DataSource.Estimated,
            nature ? Nature.Hardy : null, nature ? DataSource.Known : DataSource.Estimated,
            ivs ? StatSpread.Uniform(15) : null, ivs ? DataSource.Known : DataSource.Estimated,
            StatSpread.Uniform(0), DataSource.Known);
}