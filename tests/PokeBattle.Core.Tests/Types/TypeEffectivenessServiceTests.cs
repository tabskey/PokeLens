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
}