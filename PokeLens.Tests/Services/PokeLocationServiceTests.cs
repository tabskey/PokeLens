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
		var encountersJson = CreateEncounterPayload(new[]
		{
			("heartgold", "kanto-route-1-area"),
			("soulsilver", "kanto-route-2-area"),
			("ruby", "kanto-route-1-area")
		});

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

		var result = await service.GetLocationsByGenerationAsync("mankey", "generation-iv", "heartgold", int.MaxValue);
		result.Should().HaveCount(1);
		result.Select(r => r.Name).Should().BeEquivalentTo(new[] { "kanto-route-1-area"});
	}

	[Fact]
	public async Task GetLocationsByGenerationAsync_NoMatches_Throws()
	{
		var encountersJson = CreateEncounterPayload(new[]
		{
			("ruby", "kanto-route-3-area")
		});

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

		Func<Task> act = async () => await service.GetLocationsByGenerationAsync("pikachu", "generation-ii", "gold", int.MaxValue);
		await act.Should().ThrowAsync<Exception>()
			.WithMessage("*não pode ser encontrado na geração*");
	}

	[Fact]
	public async Task GetLocationsByGameAsync_DelegatesToGeneration()
	{
		var encountersJson = CreateEncounterPayload(new[] { ("gold", "kanto-route-1-area") });
		var client = HttpClientFactory.CreateJsonClient(req => new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent(req.RequestUri!.AbsolutePath.EndsWith("/encounters") ? encountersJson : "{}")
		}, "https://pokeapi.co/api/v2/");

		ILogger<PokeLocationService> logger = NullLogger<PokeLocationService>.Instance;
		var service = new PokeLocationService(client, logger);

		var result = await service.GetLocationsByGameAsync("pikachu", "gold", "gold", int.MaxValue);
		result.Should().HaveCount(1);
		result[0].Name.Should().Be("kanto-route-1-area");
	}

	[Fact]
	public async Task GetLocationsByGameAsync_Gen4_HeartGold()
	{
		var encountersJson = CreateEncounterPayload(new[] { ("heartgold", "johto-route-42-area") });
		var client = HttpClientFactory.CreateJsonClient(req => new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent(req.RequestUri!.AbsolutePath.EndsWith("/encounters") ? encountersJson : "{}")
		}, "https://pokeapi.co/api/v2/");

		ILogger<PokeLocationService> logger = NullLogger<PokeLocationService>.Instance;
		var service = new PokeLocationService(client, logger);

		var result = await service.GetLocationsByGameAsync("pikachu", "heartgold", "heartgold", int.MaxValue);
		result.Should().HaveCount(1);
		result[0].Name.Should().Be("johto-route-42-area");
	}

	[Fact]
	public async Task GetLocationsByGameAsync_Gen4_SoulSilver()
	{
		var encountersJson = CreateEncounterPayload(new[] { ("soulsilver", "kanto-route-28-area") });
		var client = HttpClientFactory.CreateJsonClient(req => new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent(req.RequestUri!.AbsolutePath.EndsWith("/encounters") ? encountersJson : "{}")
		}, "https://pokeapi.co/api/v2/");

		ILogger<PokeLocationService> logger = NullLogger<PokeLocationService>.Instance;
		var service = new PokeLocationService(client, logger);

		var result = await service.GetLocationsByGameAsync("pikachu", "soulsilver", "soulsilver", int.MaxValue);
		result.Should().HaveCount(1);
		result[0].Name.Should().Be("kanto-route-28-area");
	}
}


