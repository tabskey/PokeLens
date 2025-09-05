using System.Text.Json;
using FluentAssertions;
using PokeLens.Models;
using Xunit;

namespace PokeLens.Tests.Models;

public class ModelsSerializationTests
{
	[Fact]
	public void PokeSpecies_SerializesAndDeserializes()
	{
		var species = new PokeSpecies
		{
			Id = 1,
			Name = "bulbasaur",
			Abilities = new() { new PokemonAbility { Ability = new Ability { Name = "overgrow", Url = "url" }, IsHidden = false, Slot = 1 } }
		};
		var json = JsonSerializer.Serialize(species);
		var back = JsonSerializer.Deserialize<PokeSpecies>(json);
		back.Should().NotBeNull();
		back!.Id.Should().Be(1);
		back.Name.Should().Be("bulbasaur");
		back.Abilities.Should().HaveCount(1);
	}

	[Fact]
	public void PokeLocation_SerializesAndDeserializes()
	{
		var loc = new PokeLocation
		{
			LocationArea = new LocationArea { Name = "route-1", Url = "/route-1" },
			VersionDetails = new()
			{
				new VersionDetail
				{
					Version = new PokemonVersion { Name = "gold", Url = "/v/gold" },
					MaxChance = 20,
					EncounterDetails = new() { new EncounterDetail { Method = new Method { Name = "walk" }, Chance = 10 } }
				}
			}
		};
		var json = JsonSerializer.Serialize(loc);
		var back = JsonSerializer.Deserialize<PokeLocation>(json);
		back.Should().NotBeNull();
		back!.LocationArea.Name.Should().Be("route-1");
		back.VersionDetails.Should().HaveCount(1);
		back.VersionDetails[0].EncounterDetails.Should().HaveCount(1);
	}
}


