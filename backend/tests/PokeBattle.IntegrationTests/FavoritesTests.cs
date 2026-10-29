using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using PokeBattle.IntegrationTests.TestUtils;

namespace PokeBattle.IntegrationTests;

/// <summary>Saved predictions against Postgres: login required, one per question, private to each user.</summary>
[Collection(ApiTests.Name)]
public class FavoritesTests(ApiFactory factory)
{
    private const string Path = "/api/v1/favorites";

    private static readonly object Pidgey = new
    {
        target = "pidgey",
        area = "kanto-route-1-area",
        method = "walk",
        version = "firered",
        milestone = "pokedex",
        objective = "Defeat",
        scenario = "Neutral",
    };

    [Fact]
    public async Task Anonymous_IsUnauthorized()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateClient().GetAsync(Path)).StatusCode);
    }

    [Fact]
    public async Task SaveTwiceRenameAndDelete()
    {
        var client = await NewUser();

        var created = await client.PostAsJsonAsync(Path, new { prediction = Pidgey, name = "Começo" });
        var again = await client.PostAsJsonAsync(Path, new { prediction = Pidgey });

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal(HttpStatusCode.OK, again.StatusCode);
        var id = (await Json(created)).GetProperty("id").GetInt64();
        Assert.Equal(id, (await Json(again)).GetProperty("id").GetInt64());

        var list = (await Json(await client.GetAsync(Path))).EnumerateArray().ToList();
        var favorite = Assert.Single(list);
        Assert.Equal(("Começo", true), (favorite.GetProperty("name").GetString(), favorite.GetProperty("available").GetBoolean()));
        Assert.Equal("pidgey", favorite.GetProperty("prediction").GetProperty("target").GetString());
        Assert.NotEmpty(favorite.GetProperty("candidates").EnumerateArray());

        var renamed = await client.PatchAsJsonAsync($"{Path}/{id}", new { name = "Rota 1" });
        Assert.Equal("Rota 1", (await Json(renamed)).GetProperty("name").GetString());

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"{Path}/{id}")).StatusCode);
        Assert.Empty((await Json(await client.GetAsync(Path))).EnumerateArray());
    }

    [Fact]
    public async Task AnotherUsersFavorite_IsNotFound()
    {
        var owner = await NewUser();
        var id = (await Json(await owner.PostAsJsonAsync(Path, new { prediction = Pidgey }))).GetProperty("id").GetInt64();
        var intruder = await NewUser();

        Assert.Empty((await Json(await intruder.GetAsync(Path))).EnumerateArray());
        Assert.Equal(HttpStatusCode.NotFound, (await intruder.PatchAsJsonAsync($"{Path}/{id}", new { name = "meu" })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await intruder.DeleteAsync($"{Path}/{id}")).StatusCode);
    }

    [Fact]
    public async Task Save_InvalidPrediction_ReturnsValidationProblem()
    {
        var client = await NewUser();

        var response = await client.PostAsJsonAsync(Path, new { prediction = new { target = "pidgey", area = "kanto-route-1-area", method = "surf", version = "firered", milestone = "pokedex", objective = "Defeat", scenario = "Neutral" } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True((await Json(response)).GetProperty("errors").TryGetProperty("method", out _));
    }

    private async Task<HttpClient> NewUser()
    {
        var email = $"trainer-{Guid.NewGuid():N}@example.com";
        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = "pikachu-123" });
        var login = await Json(await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "pikachu-123" }));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.GetProperty("accessToken").GetString());
        return client;
    }

    private static async Task<JsonElement> Json(HttpResponseMessage response)
        => JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.Clone();
}