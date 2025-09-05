using System.Net;
using System.Net.Http;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using PokeLens.Models;
using PokeLens.Services;
using PokeLens.Tests.TestUtils;
using Xunit;

namespace PokeLens.Tests.Services;

public class PokeApiServiceTests
{
	[Fact]
	public async Task GetPokemonSpeciesAsync_Success_ReturnsSpecies()
	{
		var sample = new PokeSpecies
		{
			Id = 6,
			Name = "charizard",
			Abilities = new() { new PokemonAbility { Ability = new Ability { Name = "blaze" } } }
		};
		var json = JsonSerializer.Serialize(sample);

		var client = HttpClientFactory.CreateJsonClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent(json)
		}, "https://pokeapi.co/api/v2/");

		var service = new PokeApiService(client);
		var result = await service.GetPokemonSpeciesAsync("Charizard");
		result.Should().NotBeNull();
		result.Name.Should().Be("charizard");
		result.Abilities.Should().NotBeEmpty();
	}

	[Fact]
	public async Task GetPokemonSpeciesAsync_NotFound_Throws()
	{
		var client = HttpClientFactory.CreateJsonClient(req => new HttpResponseMessage(HttpStatusCode.NotFound), "https://pokeapi.co/api/v2/");
		var service = new PokeApiService(client);
		Func<Task> act = async () => await service.GetPokemonSpeciesAsync("missingno");
		await act.Should().ThrowAsync<Exception>()
			.Where(e => e.Message.Contains("Erro ao buscar Pokémon") || e.Message.Contains("404"));
	}
}


