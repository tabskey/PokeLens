using PokeBattle.Core.Types;

namespace PokeBattle.Core.Tests.Types;

public class MoveCategoryResolverTests
{
    [Theory]
    [InlineData(PokemonType.Normal, MoveCategory.Physical)]
    [InlineData(PokemonType.Ghost, MoveCategory.Physical)]
    [InlineData(PokemonType.Steel, MoveCategory.Physical)]
    [InlineData(PokemonType.Psychic, MoveCategory.Special)]
    [InlineData(PokemonType.Dragon, MoveCategory.Special)]
    // Bite is a Dark-type move: in Generation III, Dark is a Special type regardless of the
    // "Physical" damage_class the modern PokeAPI reports for it.
    [InlineData(PokemonType.Dark, MoveCategory.Special)]
    public void GetCategory_ReturnsGenerationIiiCategory(PokemonType moveType, MoveCategory expected)
    {
        var category = MoveCategoryResolver.GetCategory(moveType);

        Assert.Equal(expected, category);
    }

    [Fact]
    public void GetCategory_CoversAllSeventeenTypes()
    {
        foreach (var type in Enum.GetValues<PokemonType>())
        {
            MoveCategoryResolver.GetCategory(type);
        }
    }
}