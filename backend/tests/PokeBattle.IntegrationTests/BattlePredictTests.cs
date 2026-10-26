using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;

namespace PokeBattle.IntegrationTests;

/// <summary>POST /api/v1/battles/predict against the committed data/gen3-frlg package.</summary>
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
    }

    [Fact]
    public async Task Predict_InvalidRequest_ReturnsValidationProblemDetails()
    {
        var response = await factory.CreateClient().PostAsJsonAsync(Path, new
        {
            target = "pidgey",
            area = "kanto-route-1-area",
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
    public async Task Predict_UnknownEnumValue_ReturnsBadRequest()
    {
        var response = await factory.CreateClient().PostAsJsonAsync(Path, new
        {
            target = "pidgey",
            area = "kanto-route-1-area",
            version = "firered",
            milestone = "pokedex",
            objective = "Befriend",
            scenario = "Neutral",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}