using FluentAssertions;
using PokeLens.Services.mappers;
using Xunit;

namespace PokeLens.Tests.Services.Mappers;

public class GameGenerationMapperTests
{
	[Theory]
	[InlineData("red", "generation-i")]
	[InlineData("Blue", "generation-i")]
	[InlineData("gold", "generation-ii")]
	[InlineData("ruby", "generation-iii")]
	[InlineData("heartgold", "generation-iv")]
	[InlineData("soulsilver", "generation-iv")]
	[InlineData("white-2", "generation-v")]
	[InlineData("x", "generation-vi")]
	[InlineData("sun", "generation-vii")]
	[InlineData("shield", "generation-viii")]
	[InlineData("violet", "generation-ix")]
	[InlineData("the-teal-mask", "generation-ix")]
	[InlineData("the-indigo-disk", "generation-ix")]
	public void GetGenerationByGame_KnownGame_ReturnsGeneration(string game, string expected)
	{
		var result = GameGenerationMapper.GetGenerationByGame(game);
		result.Should().Be(expected);
	}

	[Fact]
	public void GetGenerationByGame_UnknownGame_Throws()
	{
		var act = () => GameGenerationMapper.GetGenerationByGame("unknown-game");
		act.Should().Throw<KeyNotFoundException>()
			.WithMessage("*unknown-game*");
	}

	[Theory]
	[InlineData("generation-i", new[] { "red", "blue", "yellow", "green" })]
	[InlineData("generation-ii", new[] { "gold", "silver", "crystal" })]
	[InlineData("generation-iii", new[] { "ruby", "sapphire", "emerald", "firered", "leafgreen", "colosseum", "xd" })]
	public void GetGamesByGeneration_ReturnsCorrectGames_ForGeneration(string generation, string[] expectedGames)
	{
		var games = GameGenerationMapper.GetGamesByGeneration(generation);
		games.Should().NotBeEmpty();
		games.Should().BeEquivalentTo(expectedGames);
	}

	[Theory]
	[InlineData("gold", true)]
	[InlineData("unknown", false)]
	[InlineData("VIOLET", true)]
	[InlineData("the-teal-mask", true)]
	public void IsGameValid_Works(string game, bool expected)
	{
		GameGenerationMapper.IsGameValid(game).Should().Be(expected);
	}

	[Fact]
	public void GetAllAvailableGames_ContainsKnownTitles()
	{
		var all = GameGenerationMapper.GetAllAvailableGames();
		all.Should().Contain(["red", "blue", "gold", "ruby", "heartgold", "soulsilver", "black", "x", "sun", "shield", "violet"
		]);
	}

	[Fact]
	public void GetAllAvailableGames_ContainsAllExpectedGames()
	{
		var all = GameGenerationMapper.GetAllAvailableGames();
		all.Length.Should().Be(GameGenerationMapper.GameToGeneration.Count);
		all.Should().Contain("the-teal-mask");
		all.Should().Contain("the-indigo-disk");
	}

	[Fact]
	public void GetGamesByGeneration_Gen4_IncludesHeartGoldAndSoulSilver()
	{
		var games = GameGenerationMapper.GetGamesByGeneration("generation-iv");
		games.Should().Contain(["heartgold", "soulsilver"]);
	}

	[Theory]
	[InlineData("growlithe", true)]
	[InlineData("arcanine", true)]
	[InlineData("mankey", true)]
	[InlineData("pikachu", false)]
	[InlineData("PHANPY", true)] // Testa case insensitive
	public void IsHeartGold_Correctly_IdentifiesExclusives(string pokemon, bool expected)
	{
		GameGenerationMapper.IsHeartGold(pokemon).Should().Be(expected);
	}

	[Theory]
	[InlineData("vulpix", true)]
	[InlineData("ninetales", true)]
	[InlineData("meowth", true)]
	[InlineData("pikachu", false)]
	[InlineData("BAGON", true)] // Testa case insensitive
	public void IsSoulSilver_Correctly_IdentifiesExclusives(string pokemon, bool expected)
	{
		GameGenerationMapper.IsSoulSilver(pokemon).Should().Be(expected);
	}

	[Theory]
	[InlineData("growlithe", true)]
	[InlineData("vulpix", true)]
	[InlineData("pikachu", false)]
	[InlineData("eevee", false)]
	public void IsHeartGoldSoulSilverExclusive_Correctly_IdentifiesExclusives(string pokemon, bool expected)
	{
		GameGenerationMapper.IsHeartGoldSoulSilverExclusive(pokemon).Should().Be(expected);
	}

	[Theory]
	[InlineData("growlithe", "HeartGold Exclusive")]
	[InlineData("vulpix", "SoulSilver Exclusive")]
	[InlineData("pikachu", "Available in both")]
	[InlineData("eevee", "Available in both")]
	public void GetHeartGoldSoulSilverAvailability_ReturnsCorrectAvailability(string pokemon, string expected)
	{
		GameGenerationMapper.GetHeartGoldSoulSilverAvailability(pokemon).Should().Be(expected);
	}
}


