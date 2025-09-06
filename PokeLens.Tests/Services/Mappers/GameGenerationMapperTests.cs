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

	[Fact]
	public void GetGamesByGeneration_ReturnsNonEmpty_ForKnownGeneration()
	{
		var games = GameGenerationMapper.GetGamesByGeneration("generation-ii");
		games.Should().NotBeEmpty();
		games.Should().Contain(new[] { "gold", "silver", "crystal" });
	}

	[Theory]
	[InlineData("gold", true)]
	[InlineData("unknown", false)]
	public void IsGameValid_Works(string game, bool expected)
	{
		GameGenerationMapper.IsGameValid(game).Should().Be(expected);
	}

	[Fact]
	public void GetAllAvailableGames_ContainsKnownTitles()
	{
		var all = GameGenerationMapper.GetAllAvailableGames();
		all.Should().Contain(new[] { "red", "blue", "gold", "ruby", "heartgold", "soulsilver", "black", "x", "sun", "shield", "violet" });
	}

	[Fact]
	public void GetGamesByGeneration_Gen4_IncludesHeartGoldAndSoulSilver()
	{
		var games = GameGenerationMapper.GetGamesByGeneration("generation-iv");
		games.Should().Contain(new[] { "heartgold", "soulsilver" });
	}

	[Fact]
	public void HeartGoldSoulSilver_Exclusivity_Checks()
	{
		GameGenerationMapper.IsHeartGold("growlithe").Should().BeTrue();
		GameGenerationMapper.IsSoulSilver("vulpix").Should().BeTrue();
		GameGenerationMapper.IsHeartGoldSoulSilverExclusive("mankey").Should().BeTrue();
		GameGenerationMapper.GetHeartGoldSoulSilverAvailability("growlithe").Should().Be("HeartGold Exclusive");
		GameGenerationMapper.GetHeartGoldSoulSilverAvailability("vulpix").Should().Be("SoulSilver Exclusive");
	}
}


