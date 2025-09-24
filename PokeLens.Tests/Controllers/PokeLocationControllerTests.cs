using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PokeLens.Controller;
using PokeLens.Models;
using PokeLens.Services;
using Xunit;
using System.Text.Json;

namespace PokeLens.Tests.Controllers;

public class PokeLocationControllerTests
{
	[Fact]
	public async Task GenerationIv_ReturnsOk_WithLocations()
	{
		var mockService = new Mock<IPokeAPILocationService>();
		mockService.Setup(s => s.GetLocationsByGenerationAsync("pikachu", "generation-iv", "heartgold", int.MaxValue))
			.ReturnsAsync(new List<LocationArea> { new LocationArea { Name = "route-1" } });

		var controller = new PokeLocationController(mockService.Object, NullLogger<PokeLocationController>.Instance);
		var result = await controller.GetPokemonLocationsGenerationIV("pikachu");

		result.Result.Should().BeOfType<OkObjectResult>();
	}

	[Fact]
	public async Task GenerationIv_HeartGold_AvailabilityText_ForHeartGoldExclusive()
	{
		var mockService = new Mock<IPokeAPILocationService>();
		mockService.Setup(s => s.GetLocationsByGenerationAsync("growlithe", "generation-iv", "heartgold", int.MaxValue))
			.ReturnsAsync(new List<LocationArea> { new LocationArea { Name = "route-36" } });

		var controller = new PokeLocationController(mockService.Object, NullLogger<PokeLocationController>.Instance);
		var actionResult = await controller.GetPokemonLocationsGenerationIV("growlithe", version: "HeartGold");
		var result = actionResult.Result as OkObjectResult;

		result.Should().NotBeNull();
		var jsonString = JsonSerializer.Serialize(result!.Value);
		using var doc = JsonDocument.Parse(jsonString);
		doc.RootElement.GetProperty("Availability").GetString().Should().Be("Available (HeartGold Exclusive)");
	}

	[Fact]
	public async Task GenerationIv_SoulSilver_AvailabilityText_ForSoulSilverExclusive()
	{
		var mockService = new Mock<IPokeAPILocationService>();
		mockService.Setup(s => s.GetLocationsByGenerationAsync("vulpix", "generation-iv", "soulsilver", int.MaxValue))
			.ReturnsAsync(new List<LocationArea> { new LocationArea { Name = "route-8" } });

		var controller = new PokeLocationController(mockService.Object, NullLogger<PokeLocationController>.Instance);
		var actionResult = await controller.GetPokemonLocationsGenerationIV("vulpix", version: "SoulSilver");
		var result = actionResult.Result as OkObjectResult;

		result.Should().NotBeNull();
		var jsonString = JsonSerializer.Serialize(result!.Value);
		using var doc = JsonDocument.Parse(jsonString);
		doc.RootElement.GetProperty("Availability").GetString().Should().Be("Available (SoulSilver Exclusive)");
	}

	[Fact]
	public async Task ByGame_BadGame_ReturnsBadRequest()
	{
		var mockService = new Mock<IPokeAPILocationService>();
		var controller = new PokeLocationController(mockService.Object, NullLogger<PokeLocationController>.Instance);

		var result = await controller.GetPokemonLocationsByGame("pikachu", "bad-game");
		result.Should().BeOfType<BadRequestObjectResult>();
	}

	[Fact]
	public async Task ByGame_Valid_ReturnsOk()
	{
		var mockService = new Mock<IPokeAPILocationService>();
		mockService.Setup(s => s.GetLocationsByGameAsync("pikachu", "heartgold", "heartgold", int.MaxValue))
			.ReturnsAsync(new List<LocationArea> { new LocationArea { Name = "route-1" } });

		var controller = new PokeLocationController(mockService.Object, NullLogger<PokeLocationController>.Instance);
		var result = await controller.GetPokemonLocationsByGame("pikachu", "heartgold");
		result.Should().BeOfType<OkObjectResult>();
	}

	[Fact]
	public async Task GenerationIv_NonExclusive_AvailableInBothGames()
	{
		var mockService = new Mock<IPokeAPILocationService>();
		mockService
			.Setup(s => s.GetLocationsByGenerationAsync("pikachu", "generation-iv", It.IsAny<string>(), It.IsAny<int>()))
			.ReturnsAsync(new List<LocationArea> { new LocationArea { Name = "route-29" } });

		var controller = new PokeLocationController(mockService.Object, NullLogger<PokeLocationController>.Instance);
		var actionResultHg = await controller.GetPokemonLocationsGenerationIV("pikachu", version: "HeartGold");
		var actionResultSs = await controller.GetPokemonLocationsGenerationIV("pikachu", version: "SoulSilver");
		var resultHg = actionResultHg.Result as OkObjectResult;
		var resultSs = actionResultSs.Result as OkObjectResult;

		resultHg.Should().NotBeNull();
		resultSs.Should().NotBeNull();

		string ExtractAvailability(OkObjectResult ok)
		{
			var jsonString = JsonSerializer.Serialize(ok.Value);
			using var doc = JsonDocument.Parse(jsonString);
			return doc.RootElement.GetProperty("Availability").GetString()!;
		}

		ExtractAvailability(resultHg!).Should().Be("Available in both games");
		ExtractAvailability(resultSs!).Should().Be("Available in both games");
	}

	[Fact]
	public async Task GenerationIv_NotFound_ReturnsNotFoundPayload()
	{
		var mockService = new Mock<IPokeAPILocationService>();
		mockService
			.Setup(s => s.GetLocationsByGenerationAsync("missingmon", "generation-iv", It.IsAny<string>(), It.IsAny<int>()))
			.ThrowsAsync(new Exception("Pokémon missingmon não encontrado na geração"));

		var controller = new PokeLocationController(mockService.Object, NullLogger<PokeLocationController>.Instance);
		var actionResult = await controller.GetPokemonLocationsGenerationIV("missingmon");
		var result = actionResult.Result as NotFoundObjectResult;

		result.Should().NotBeNull();
		var jsonString = JsonSerializer.Serialize(result!.Value);
		using var doc = JsonDocument.Parse(jsonString);
		doc.RootElement.GetProperty("Error").GetString().Should().Contain("missingmon");
		doc.RootElement.GetProperty("Pokemon").GetString().Should().Be("missingmon");
		doc.RootElement.GetProperty("Generation").GetString().Should().Be("generation-iv");
	}
}




