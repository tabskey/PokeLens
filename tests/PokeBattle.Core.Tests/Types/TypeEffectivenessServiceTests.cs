using PokeBattle.Core.Types;

namespace PokeBattle.Core.Tests.Types;

public class TypeEffectivenessServiceTests
{
    [Theory]
    [InlineData(PokemonType.Normal, PokemonType.Ghost, 0)]
    [InlineData(PokemonType.Normal, PokemonType.Rock, 0.5)]
    [InlineData(PokemonType.Fighting, PokemonType.Normal, 2)]
    [InlineData(PokemonType.Water, PokemonType.Water, 0.5)]
    [InlineData(PokemonType.Electric, PokemonType.Ground, 0)]
    [InlineData(PokemonType.Dark, PokemonType.Psychic, 2)]
    [InlineData(PokemonType.Grass, PokemonType.Fire, 0.5)]
    [InlineData(PokemonType.Normal, PokemonType.Fire, 1)]
    public void GetMultiplier_SingleType_MatchesGenerationIiiChart(PokemonType attacker, PokemonType defender, double expected)
    {
        var multiplier = TypeEffectivenessService.GetMultiplier(attacker, defender);

        Assert.Equal(expected, multiplier);
    }

    [Fact]
    public void GetMultiplier_DualType_Multiplies025x()
    {
        // Grass vs a Fire/Flying defender: 0.5 (Fire resists Grass) x 0.5 (Flying resists Grass) = 0.25
        var multiplier = TypeEffectivenessService.GetMultiplier(PokemonType.Grass, PokemonType.Fire, PokemonType.Flying);

        Assert.Equal(0.25, multiplier);
    }

    [Fact]
    public void GetMultiplier_DualType_Multiplies4x()
    {
        // Ground vs a Fire/Rock defender: 2 (Fire weak to Ground) x 2 (Rock weak to Ground) = 4
        var multiplier = TypeEffectivenessService.GetMultiplier(PokemonType.Ground, PokemonType.Fire, PokemonType.Rock);

        Assert.Equal(4.0, multiplier);
    }

    [Fact]
    public void GetMultiplier_GhostAgainstNormal_IsZero()
    {
        var multiplier = TypeEffectivenessService.GetMultiplier(PokemonType.Ghost, PokemonType.Normal);

        Assert.Equal(0, multiplier);
    }

    [Fact]
    public void GetMultiplier_SteelResistsGhostAndDark_LikePreGenVi()
    {
        Assert.Equal(0.5, TypeEffectivenessService.GetMultiplier(PokemonType.Ghost, PokemonType.Steel));
        Assert.Equal(0.5, TypeEffectivenessService.GetMultiplier(PokemonType.Dark, PokemonType.Steel));
    }

    // Both entries were missing from the first version of the chart (checked against gTypeEffectiveness).
    [Theory]
    [InlineData(PokemonType.Ghost, PokemonType.Psychic)]
    [InlineData(PokemonType.Ice, PokemonType.Grass)]
    public void GetMultiplier_EntriesPresentInGameChart_AreSuperEffective(PokemonType attacker, PokemonType defender)
    {
        Assert.Equal(2, TypeEffectivenessService.GetMultiplier(attacker, defender));
    }

    // Fire vs Bug/Rock with damage 15. The game chart lists Fire->Bug (2x) before Fire->Rock (0.5x):
    // 15*20/10 = 30; 30*5/10 = 15. The opposite order would give 15*5/10 = 7; 7*20/10 = 14.
    // The defender's type order (Bug/Rock or Rock/Bug) does not change the chart order.
    [Theory]
    [InlineData(PokemonType.Bug, PokemonType.Rock)]
    [InlineData(PokemonType.Rock, PokemonType.Bug)]
    public void ApplyTo_MixedEffectiveness_FollowsGameChartOrder(PokemonType type1, PokemonType type2)
    {
        Assert.Equal(15, TypeEffectivenessService.ApplyTo(15, PokemonType.Fire, type1, type2));
    }

    // Ice vs Fire/Flying with damage 19: Ice->Flying (2x) comes before Ice->Fire (0.5x) in the chart,
    // so 19*20/10 = 38; 38*5/10 = 19 (not 19*5/10 = 9; 9*20/10 = 18).
    [Fact]
    public void ApplyTo_IceAgainstFireFlying_AppliesFlyingFirst()
    {
        Assert.Equal(19, TypeEffectivenessService.ApplyTo(19, PokemonType.Ice, PokemonType.Fire, PokemonType.Flying));
    }

    // A monotype stored as type1 == type2 must only be applied once (the game skips type2 in that case).
    [Fact]
    public void ApplyTo_SameTypeTwice_AppliesOnce()
    {
        Assert.Equal(20, TypeEffectivenessService.ApplyTo(10, PokemonType.Water, PokemonType.Fire, PokemonType.Fire));
    }

    [Fact]
    public void ApplyTo_Resisted_NeverDropsBelowOne()
    {
        Assert.Equal(1, TypeEffectivenessService.ApplyTo(1, PokemonType.Fire, PokemonType.Water, null));
    }

    [Fact]
    public void ApplyTo_Immune_ReturnsZero()
    {
        Assert.Equal(0, TypeEffectivenessService.ApplyTo(50, PokemonType.Normal, PokemonType.Ghost, null));
    }
}