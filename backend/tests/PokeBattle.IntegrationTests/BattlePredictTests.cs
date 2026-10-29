using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;

namespace PokeBattle.IntegrationTests;

/// <summary>The battle endpoints against the committed data/gen3-frlg package.</summary>
public class BattlePredictTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Path = "/api/v1/battles/predict";

    [Theory]
    [InlineData("Defeat")]
    [InlineData("Catch")]
    public async Task Predict_PidgeyOnRoute1_ReturnsUpToFiveRankedCandidates(string objective)
    {
        var response = await factory.CreateClient().PostAsJsonAsync(Path, new
        {
            target = "pidgey",
            area = "kanto-route-1-area",
            method = "walk",
            version = "firered",
            milestone = "pokedex",
            objective,
            scenario = "Pessimistic",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var candidates = body.RootElement.GetProperty("candidates");
        Assert.InRange(candidates.GetArrayLength(), 1, 5);
        Assert.All(candidates.EnumerateArray(), candidate =>
        {
            var scenarios = candidate.GetProperty("scenarios").EnumerateArray().Select(s => s.GetProperty("scenario").GetString());
            Assert.Equal(["Pessimistic", "Neutral", "Optimistic"], scenarios);
        });
        Assert.Equal("Pessimistic", body.RootElement.GetProperty("rankedBy").GetString());
        Assert.Equal(16, body.RootElement.GetProperty("targetId").GetInt32());
    }

    [Fact]
    public async Task Predict_InvalidRequest_ReturnsValidationProblemDetails()
    {
        var response = await factory.CreateClient().PostAsJsonAsync(Path, new
        {
            target = "pidgey",
            area = "kanto-route-1-area",
            method = "walk",
            version = "ruby",
            milestone = "pokedex",
            objective = "Defeat",
            scenario = "Neutral",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(body.RootElement.GetProperty("errors").TryGetProperty("version", out _));
    }

    [Fact]
    public async Task Predict_MethodNotUnlocked_ReturnsValidationProblemDetails()
    {
        var response = await factory.CreateClient().PostAsJsonAsync(Path, new
        {
            target = "magikarp",
            area = "kanto-route-24-area",
            method = "old-rod",
            version = "firered",
            milestone = "boulder-badge",
            objective = "Defeat",
            scenario = "Neutral",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(body.RootElement.GetProperty("errors").TryGetProperty("method", out _));
    }

    [Fact]
    public async Task Options_ListsMilestonesInGameOrder()
    {
        var response = await factory.CreateClient().GetAsync("/api/v1/game-data/options");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("start", body.RootElement.GetProperty("milestones")[0].GetString());
    }

    [Fact]
    public async Task Targets_PidgeyAtPokedex_HasOneOptionPerUnlockedArea()
    {
        var response = await factory.CreateClient().GetAsync("/api/v1/targets?version=firered&milestone=pokedex");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var pidgey = body.RootElement.EnumerateArray().Single(t => t.GetProperty("pokemon").GetString() == "pidgey");
        Assert.Equal(16, pidgey.GetProperty("id").GetInt32());
        var route1 = pidgey.GetProperty("encounters").EnumerateArray().First();
        Assert.Equal(("kanto-route-1-area", "Route 1", "walk"), (route1.GetProperty("area").GetString(), route1.GetProperty("displayName").GetString(), route1.GetProperty("method").GetString()));
        Assert.Equal((2, 5), (route1.GetProperty("minLevel").GetInt32(), route1.GetProperty("maxLevel").GetInt32()));
        Assert.True(pidgey.GetProperty("encounters").GetArrayLength() > 1);
        Assert.DoesNotContain(body.RootElement.EnumerateArray(), t => t.GetProperty("pokemon").GetString() == "geodude");
    }

    [Fact]
    public async Task Targets_UnknownVersion_ReturnsValidationProblemDetails()
    {
        var response = await factory.CreateClient().GetAsync("/api/v1/targets?version=ruby&milestone=start");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Predict_UnknownEnumValue_ReturnsBadRequest()
    {
        var response = await factory.CreateClient().PostAsJsonAsync(Path, new
        {
            target = "pidgey",
            area = "kanto-route-1-area",
            method = "walk",
            version = "firered",
            milestone = "pokedex",
            objective = "Befriend",
            scenario = "Neutral",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}