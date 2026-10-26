using System.Text.Json;

using PokeBattle.Application.GameData;
using PokeBattle.Core.Battles;
using PokeBattle.Core.Types;
using PokeBattle.DataImporter;
using PokeBattle.DataImporter.PokeApi;

namespace PokeBattle.DataImporter.Tests;

// Fixtures are trimmed copies of real PokeAPI payloads (fetched 2026-10-25), keeping only the fields the importer reads.
public class Gen3NormalizerTests
{
    private static readonly Dictionary<string, int> VersionGroupOrder = new()
    {
        ["gold-silver"] = 3,
        ["firered-leafgreen"] = 11,
        ["black-white"] = 14,
        ["x-y"] = 17,
    };

    [Fact]
    public void NormalizeMove_Bite_IsDarkAndSpecial_IgnoringPreGen3TypeAndDamageClass()
    {
        // Bite was Normal through Gold/Silver (past_values) and PokeAPI's damage_class says physical.
        var bite = Parse<MoveResponse>("""
            {
              "id": 44, "name": "bite", "power": 60, "accuracy": 100, "pp": 25,
              "damage_class": { "name": "physical", "url": "https://pokeapi.co/api/v2/move-damage-class/2/" },
              "type": { "name": "dark", "url": "https://pokeapi.co/api/v2/type/17/" },
              "past_values": [
                { "power": null, "accuracy": null, "pp": null,
                  "type": { "name": "normal", "url": "https://pokeapi.co/api/v2/type/1/" },
                  "version_group": { "name": "gold-silver", "url": "https://pokeapi.co/api/v2/version-group/3/" } }
              ],
              "meta": { "crit_rate": 0 },
              "machines": []
            }
            """);

        var move = Gen3Normalizer.NormalizeMove(bite, VersionGroupOrder, machine: null);

        Assert.NotNull(move);
        Assert.Equal(PokemonType.Dark, move.Type);
        Assert.Equal(MoveCategory.Special, move.Category);
        Assert.Equal(60, move.Power);
    }

    [Fact]
    public void NormalizeMove_ValuesChangedAfterGen3_UsesGen3Values()
    {
        // Thief: 40 power / 10 PP through Gen IV; 60 / 25 from Black/White on.
        var thief = Parse<MoveResponse>("""
            {
              "id": 168, "name": "thief", "power": 60, "accuracy": 100, "pp": 25,
              "type": { "name": "dark", "url": "https://pokeapi.co/api/v2/type/17/" },
              "past_values": [
                { "power": 40, "accuracy": null, "pp": 10, "type": null,
                  "version_group": { "name": "black-white", "url": "https://pokeapi.co/api/v2/version-group/11/" } }
              ],
              "meta": { "crit_rate": 0 },
              "machines": []
            }
            """);

        var move = Gen3Normalizer.NormalizeMove(thief, VersionGroupOrder, machine: "tm46");

        Assert.NotNull(move);
        Assert.Equal(40, move.Power);
        Assert.Equal(10, move.Pp);
        Assert.Equal(100, move.Accuracy);
        Assert.Equal("tm46", move.Machine);
    }

    [Theory]
    [InlineData("null", "[]")]
    [InlineData("60", """[{ "power": 1, "accuracy": null, "pp": null, "type": null, "version_group": { "name": "x-y", "url": "https://pokeapi.co/api/v2/version-group/15/" } }]""")]
    public void NormalizeMove_StatusOrVariablePower_IsExcluded(string power, string pastValues)
    {
        var response = Parse<MoveResponse>($$"""
            {
              "id": 237, "name": "hidden-power", "power": {{power}}, "accuracy": 100, "pp": 15,
              "type": { "name": "normal", "url": "https://pokeapi.co/api/v2/type/1/" },
              "past_values": {{pastValues}},
              "meta": { "crit_rate": 0 },
              "machines": []
            }
            """);

        Assert.Null(Gen3Normalizer.NormalizeMove(response, VersionGroupOrder, machine: null));
    }

    [Fact]
    public void NormalizeMove_MultiHit_IsExcluded()
    {
        // Bullet Seed hits 2 to 5 times; Gen III power 10 (25 from Gen V on).
        var bulletSeed = Parse<MoveResponse>("""
            {
              "id": 331, "name": "bullet-seed", "power": 25, "accuracy": 100, "pp": 30,
              "type": { "name": "grass", "url": "https://pokeapi.co/api/v2/type/12/" },
              "past_values": [],
              "meta": { "crit_rate": 0, "min_hits": 2, "max_hits": 5 },
              "machines": []
            }
            """);

        Assert.Null(Gen3Normalizer.NormalizeMove(bulletSeed, VersionGroupOrder, machine: null));
    }

    [Fact]
    public void NormalizePokemon_Butterfree_DropsHiddenAbilityAndUsesGen3BaseStats()
    {
        var butterfree = Parse<PokemonResponse>("""
            {
              "id": 12, "name": "butterfree",
              "types": [
                { "slot": 1, "type": { "name": "bug", "url": "https://pokeapi.co/api/v2/type/7/" } },
                { "slot": 2, "type": { "name": "flying", "url": "https://pokeapi.co/api/v2/type/3/" } }
              ],
              "past_types": [],
              "stats": [
                { "base_stat": 60, "stat": { "name": "hp", "url": "https://pokeapi.co/api/v2/stat/1/" } },
                { "base_stat": 45, "stat": { "name": "attack", "url": "https://pokeapi.co/api/v2/stat/2/" } },
                { "base_stat": 50, "stat": { "name": "defense", "url": "https://pokeapi.co/api/v2/stat/3/" } },
                { "base_stat": 90, "stat": { "name": "special-attack", "url": "https://pokeapi.co/api/v2/stat/4/" } },
                { "base_stat": 80, "stat": { "name": "special-defense", "url": "https://pokeapi.co/api/v2/stat/5/" } },
                { "base_stat": 70, "stat": { "name": "speed", "url": "https://pokeapi.co/api/v2/stat/6/" } }
              ],
              "past_stats": [
                { "generation": { "name": "generation-i", "url": "https://pokeapi.co/api/v2/generation/1/" },
                  "stats": [ { "base_stat": 80, "stat": { "name": "special", "url": "https://pokeapi.co/api/v2/stat/9/" } } ] },
                { "generation": { "name": "generation-v", "url": "https://pokeapi.co/api/v2/generation/5/" },
                  "stats": [ { "base_stat": 80, "stat": { "name": "special-attack", "url": "https://pokeapi.co/api/v2/stat/4/" } } ] }
              ],
              "abilities": [
                { "is_hidden": false, "slot": 1, "ability": { "name": "compound-eyes", "url": "https://pokeapi.co/api/v2/ability/14/" } },
                { "is_hidden": true, "slot": 3, "ability": { "name": "tinted-lens", "url": "https://pokeapi.co/api/v2/ability/110/" } }
              ],
              "past_abilities": [
                { "generation": { "name": "generation-iv", "url": "https://pokeapi.co/api/v2/generation/4/" },
                  "abilities": [ { "is_hidden": true, "slot": 3, "ability": null } ] }
              ],
              "moves": [
                { "move": { "name": "gust", "url": "https://pokeapi.co/api/v2/move/16/" },
                  "version_group_details": [
                    { "level_learned_at": 13, "move_learn_method": { "name": "level-up", "url": "https://pokeapi.co/api/v2/move-learn-method/1/" },
                      "version_group": { "name": "firered-leafgreen", "url": "https://pokeapi.co/api/v2/version-group/7/" } },
                    { "level_learned_at": 12, "move_learn_method": { "name": "level-up", "url": "https://pokeapi.co/api/v2/move-learn-method/1/" },
                      "version_group": { "name": "x-y", "url": "https://pokeapi.co/api/v2/version-group/15/" } }
                  ] },
                { "move": { "name": "bug-buzz", "url": "https://pokeapi.co/api/v2/move/405/" },
                  "version_group_details": [
                    { "level_learned_at": 40, "move_learn_method": { "name": "level-up", "url": "https://pokeapi.co/api/v2/move-learn-method/1/" },
                      "version_group": { "name": "x-y", "url": "https://pokeapi.co/api/v2/version-group/15/" } }
                  ] },
                { "move": { "name": "psybeam", "url": "https://pokeapi.co/api/v2/move/60/" },
                  "version_group_details": [
                    { "level_learned_at": 34, "move_learn_method": { "name": "level-up", "url": "https://pokeapi.co/api/v2/move-learn-method/1/" },
                      "version_group": { "name": "x-y", "url": "https://pokeapi.co/api/v2/version-group/15/" } }
                  ] }
              ]
            }
            """);

        var species = Gen3Normalizer.NormalizePokemon(butterfree, captureRate: 45, knownMoves: new HashSet<string> { "gust", "psybeam" });

        Assert.Equal(["compound-eyes"], species.Abilities);
        Assert.Equal([PokemonType.Bug, PokemonType.Flying], species.Types);
        Assert.Equal(new StatSpread(60, 45, 50, 80, 80, 70), species.BaseStats);
        // Only FRLG entries of moves in the package: psybeam is X/Y-only here, bug-buzz is not a Gen III move.
        Assert.Equal([new("gust", LearnMethod.LevelUp, 13)], species.Learnset);
    }

    [Fact]
    public void NormalizePokemon_PastTypesAndAbilities_AppliedFromLaterGenerations()
    {
        // Clefairy's types (Normal through Gen V) with Gengar's abilities (Levitate through Gen VI) in one fixture.
        var pokemon = Parse<PokemonResponse>("""
            {
              "id": 35, "name": "clefairy",
              "types": [ { "slot": 1, "type": { "name": "fairy", "url": "https://pokeapi.co/api/v2/type/18/" } } ],
              "past_types": [
                { "generation": { "name": "generation-v", "url": "https://pokeapi.co/api/v2/generation/5/" },
                  "types": [ { "slot": 1, "type": { "name": "normal", "url": "https://pokeapi.co/api/v2/type/1/" } } ] }
              ],
              "stats": [
                { "base_stat": 70, "stat": { "name": "hp", "url": "" } }, { "base_stat": 45, "stat": { "name": "attack", "url": "" } },
                { "base_stat": 48, "stat": { "name": "defense", "url": "" } }, { "base_stat": 60, "stat": { "name": "special-attack", "url": "" } },
                { "base_stat": 65, "stat": { "name": "special-defense", "url": "" } }, { "base_stat": 35, "stat": { "name": "speed", "url": "" } }
              ],
              "past_stats": [],
              "abilities": [ { "is_hidden": false, "slot": 1, "ability": { "name": "cursed-body", "url": "https://pokeapi.co/api/v2/ability/130/" } } ],
              "past_abilities": [
                { "generation": { "name": "generation-vi", "url": "https://pokeapi.co/api/v2/generation/6/" },
                  "abilities": [ { "is_hidden": false, "slot": 1, "ability": { "name": "levitate", "url": "https://pokeapi.co/api/v2/ability/26/" } } ] }
              ],
              "moves": []
            }
            """);

        var species = Gen3Normalizer.NormalizePokemon(pokemon, captureRate: 150, knownMoves: new HashSet<string>());

        Assert.Equal([PokemonType.Normal], species.Types);
        Assert.Equal(["levitate"], species.Abilities);
    }

    [Fact]
    public void NormalizeEncounters_KeepsOnlyFireRedAndLeafGreen()
    {
        var area = Parse<LocationAreaResponse>("""
            {
              "name": "kanto-route-1-area",
              "location": { "name": "kanto-route-1", "url": "https://pokeapi.co/api/v2/location/88/" },
              "pokemon_encounters": [
                { "pokemon": { "name": "pidgey", "url": "https://pokeapi.co/api/v2/pokemon/16/" },
                  "version_details": [
                    { "version": { "name": "firered", "url": "https://pokeapi.co/api/v2/version/10/" },
                      "encounter_details": [ { "min_level": 2, "max_level": 2, "chance": 10, "condition_values": [],
                        "method": { "name": "walk", "url": "https://pokeapi.co/api/v2/encounter-method/1/" } } ] },
                    { "version": { "name": "red", "url": "https://pokeapi.co/api/v2/version/1/" },
                      "encounter_details": [ { "min_level": 2, "max_level": 5, "chance": 55, "condition_values": [],
                        "method": { "name": "walk", "url": "https://pokeapi.co/api/v2/encounter-method/1/" } } ] }
                  ] }
              ]
            }
            """);

        var encounter = Assert.Single(Gen3Normalizer.NormalizeEncounters(area));

        Assert.Equal(("kanto-route-1", "firered", "pidgey", "walk", 2, 2, 10),
            (encounter.Location, encounter.Version, encounter.Pokemon, encounter.Method, encounter.MinLevel, encounter.MaxLevel, encounter.Chance));
    }

    private static T Parse<T>(string json) => JsonSerializer.Deserialize<T>(json, PokeApiClient.JsonOptions)!;
}