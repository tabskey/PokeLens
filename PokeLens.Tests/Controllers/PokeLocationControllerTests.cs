using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PokeLens.Controller;
using PokeLens.Models;
using PokeLens.Services;
using Xunit;

namespace PokeLens.Tests.Controllers;

public class PokeLocationControllerTests
{
	[Fact]
	public async Task GenerationIi_ReturnsOk_WithLocations()
	{
		var mockService = new Mock<IPokeAPILocationService>();
		mockService.Setup(s => s.GetLocationsByGenerationAsync("pikachu", "generation-iv"))
			.ReturnsAsync(new List<LocationArea> { new LocationArea { Name = "route-1" } });

		var controller = new PokeLocationController(mockService.Object, NullLogger<PokeLocationController>.Instance);
		var result = await controller.GetPokemonLocationsGenerationIV("pikachu");

		result.Should().BeOfType<OkObjectResult>();
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
		mockService.Setup(s => s.GetLocationsByGameAsync("pikachu", "gold"))
			.ReturnsAsync(new List<LocationArea> { new LocationArea { Name = "route-1" } });

		var controller = new PokeLocationController(mockService.Object, NullLogger<PokeLocationController>.Instance);
		var result = await controller.GetPokemonLocationsByGame("pikachu", "heartgold");
		result.Should().BeOfType<OkObjectResult>();
	}
}




