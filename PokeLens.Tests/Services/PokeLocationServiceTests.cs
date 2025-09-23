using System.Net;
using System.Net.Http;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PokeLens.Models;
using PokeLens.Services;
using PokeLens.Tests.TestUtils;
using Xunit;

namespace PokeLens.Tests.Services;

public class PokeLocationServiceTests
{
	private static string CreateEncounterPayload(IEnumerable<(string version, string locationName)> items)
	{
		var payload = items
			.GroupBy(i => i.locationName)
			.Select(g => new PokeLocation
			{
				LocationArea = new LocationArea { Name = g.Key, Url = $"/location-area/{g.Key}" },
				VersionDetails = g.Select(i => new VersionDetail
				{
					Version = new PokemonVersion { Name = i.version }
				}).ToList()
			});
		return JsonSerializer.Serialize(payload);
	}

	[Fact]
	public async Task GetLocationsByGenerationAsync_FiltersByGeneration()
	{
		var encountersJson = CreateEncounterPayload([
			("heartgold", "route-1"),
			("soulsilver", "route-2"),
			("ruby", "route-3")
		]);

		var client = HttpClientFactory.CreateJsonClient(req =>
		{
			var path = req.RequestUri!.AbsolutePath;
			if (path.Contains("/pokemon/") && !path.EndsWith("/encounters"))
			{
				return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
			}
			if (path.EndsWith("/encounters"))
			{
				return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(encountersJson) };
			}
			return new HttpResponseMessage(HttpStatusCode.BadRequest);
		}, "https://pokeapi.co/api/v2/");

		ILogger<PokeLocationService> logger = NullLogger<PokeLocationService>.Instance;
		var service = new PokeLocationService(client, logger);

		var result = await service.GetLocationsByGenerationAsync("mankey", "generation-iv");
		result.Should().HaveCount(2);
		result.Select(r => r.Name).Should().BeEquivalentTo(new[] { "route-1", "route-2" });
	}

	[Fact]
	// futuros testes
	public async Task GetLocationsByGenerationAsync_AcceptsNumericGeneration()
	{
		var encountersJson = CreateEncounterPayload([
			("heartgold", "route-1"),
			("soulsilver", "route-2"),
			("ruby", "route-3")
		]);
	}

	[Fact]
	public async Task GetLocationsByGenerationAsync_NoMatches_Throws()
	{
		var encountersJson = CreateEncounterPayload([
			("ruby", "route-3")
		]);

		var client = HttpClientFactory.CreateJsonClient(req =>
		{
			var path = req.RequestUri!.AbsolutePath;
			if (path.Contains("/pokemon/") && !path.EndsWith("/encounters"))
			{
				return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
			}
			if (path.EndsWith("/encounters"))
			{
				return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(encountersJson) };
			}
			return new HttpResponseMessage(HttpStatusCode.BadRequest);
		}, "https://pokeapi.co/api/v2/");

		ILogger<PokeLocationService> logger = NullLogger<PokeLocationService>.Instance;
		var service = new PokeLocationService(client, logger);

		Func<Task> act = async () => await service.GetLocationsByGenerationAsync("pikachu", "generation-iv");
		await act.Should().ThrowAsync<Exception>()
			.WithMessage("*Pokemon Pikachu cannot be found in generation*");
	}

[Fact]
public async Task GetLocationsByGenerationAsync_InvalidGeneration_Throws()
{

    var validEmptyEncountersJson = "[]";
    
    var client = HttpClientFactory.CreateJsonClient(req => 
    {
        var path = req.RequestUri!.AbsolutePath;
        if (path.Contains("/pokemon/") && !path.EndsWith("/encounters"))
        {
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
        }
        else if (path.EndsWith("/encounters"))
        {
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(validEmptyEncountersJson) };
        }
        return new HttpResponseMessage(HttpStatusCode.BadRequest);
    }, "https://pokeapi.co/api/v2/");
    
    ILogger<PokeLocationService> logger = NullLogger<PokeLocationService>.Instance;
    var service = new PokeLocationService(client, logger);
    
    Func<Task> act = async () => await service.GetLocationsByGenerationAsync("pikachu", "generation-99");
    await act.Should().ThrowAsync<Exception>()
        .WithMessage("*Pokemon pikachu cannot be found in generation generation-99*");
}
	[Fact]
	public async Task GetLocationsByGenerationAsync_PokemonNotFound_Throws()
	{
		var client = HttpClientFactory.CreateJsonClient(req =>
		{
			if (req.RequestUri!.AbsolutePath.Contains("/pokemon/"))
			{
				return new HttpResponseMessage(HttpStatusCode.NotFound);
			}
			return new HttpResponseMessage(HttpStatusCode.OK);
		}, "https://pokeapi.co/api/v2/");
		
		ILogger<PokeLocationService> logger = NullLogger<PokeLocationService>.Instance;
		var service = new PokeLocationService(client, logger);
		
		Func<Task> act = async () => await service.GetLocationsByGenerationAsync("missing-pokemon", "generation-iv");
		await act.Should().ThrowAsync<Exception>()
			.WithMessage("*Pokemon missing-pokemon not found*");
	}

	[Fact]
	public async Task GetLocationsByGameAsync_DelegatesToGeneration()
	{
		var encountersJson = CreateEncounterPayload([("gold", "route-1")]);
		var client = HttpClientFactory.CreateJsonClient(req => new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent(req.RequestUri!.AbsolutePath.EndsWith("/encounters") ? encountersJson : "{}")
		}, "https://pokeapi.co/api/v2/");

		ILogger<PokeLocationService> logger = NullLogger<PokeLocationService>.Instance;
		var service = new PokeLocationService(client, logger);

		var result = await service.GetLocationsByGameAsync("pikachu", "gold");
		result.Should().HaveCount(1);
		result[0].Name.Should().Be("route-1");
	}

	[Fact]
	public async Task GetLocationsByGameAsync_InvalidGame_Throws()
	{
		var client = HttpClientFactory.CreateJsonClient(_ => 
			new HttpResponseMessage(HttpStatusCode.OK));
		
		ILogger<PokeLocationService> logger = NullLogger<PokeLocationService>.Instance;
		var service = new PokeLocationService(client, logger);
		
		Func<Task> act = async () => await service.GetLocationsByGameAsync("pikachu", "invalid-game");
		await act.Should().ThrowAsync<KeyNotFoundException>()
			.WithMessage("*invalid-game*");
	}

	[Fact]
	public async Task GetLocationsByGameAsync_Gen4_HeartGold()
	{
		var encountersJson = CreateEncounterPayload([("heartgold", "route-42")]);
		var client = HttpClientFactory.CreateJsonClient(req => new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent(req.RequestUri!.AbsolutePath.EndsWith("/encounters") ? encountersJson : "{}")
		}, "https://pokeapi.co/api/v2/");

		ILogger<PokeLocationService> logger = NullLogger<PokeLocationService>.Instance;
		var service = new PokeLocationService(client, logger);

		var result = await service.GetLocationsByGameAsync("pikachu", "heartgold");
		result.Should().HaveCount(1);
		result[0].Name.Should().Be("route-42");
	}

	[Fact]
	public async Task GetLocationsByGameAsync_Gen4_SoulSilver()
	{
		var encountersJson = CreateEncounterPayload([("soulsilver", "route-28")]);
		var client = HttpClientFactory.CreateJsonClient(req => new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent(req.RequestUri!.AbsolutePath.EndsWith("/encounters") ? encountersJson : "{}")
		}, "https://pokeapi.co/api/v2/");

		ILogger<PokeLocationService> logger = NullLogger<PokeLocationService>.Instance;
		var service = new PokeLocationService(client, logger);

		var result = await service.GetLocationsByGameAsync("pikachu", "soulsilver");
		result.Should().HaveCount(1);
		result[0].Name.Should().Be("route-28");
	}
}


