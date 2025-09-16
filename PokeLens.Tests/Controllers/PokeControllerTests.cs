
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PokeLens.Controller;
using PokeLens.Models;
using PokeLens.Services;
using Xunit;

namespace PokeLens.Tests.Controllers;

public class PokeControllerTests
{
	[Fact]
	public async Task GetPokemonSpecies_Success_ReturnsOk()
	{
		var mockService = new Mock<IPokeApiService>();
		mockService.Setup(s => s.GetPokemonSpeciesAsync("pikachu"))
			.ReturnsAsync(new PokeSpecies { Id = 25, Name = "pikachu" });

		var logger = NullLogger<PokeController>.Instance;
		var dummyLocationService = new PokeLocationService(new HttpClient(), NullLogger<PokeLocationService>.Instance);
		var controller = new PokeController(mockService.Object, dummyLocationService, logger);

		var result = await controller.GetPokemonSpecies("pikachu");
		result.Result.Should().BeOfType<OkObjectResult>();
		var ok = (OkObjectResult)result.Result!;
		ok.Value.Should().BeOfType<PokeSpecies>();
		((PokeSpecies)ok.Value!).Name.Should().Be("pikachu");
	}

	[Fact]
	public async Task GetPokemonSpecies_NotFound_Returns404()
	{
		var mockService = new Mock<IPokeApiService>();
		mockService.Setup(s => s.GetPokemonSpeciesAsync("missing"))
			.ThrowsAsync(new Exception("não encontrado"));

		var logger = NullLogger<PokeController>.Instance;
		var dummyLocationService = new PokeLocationService(new HttpClient(), NullLogger<PokeLocationService>.Instance);
		var controller = new PokeController(mockService.Object, dummyLocationService, logger);

		var result = await controller.GetPokemonSpecies("missing");
		result.Result.Should().BeOfType<NotFoundObjectResult>();
	}
}
