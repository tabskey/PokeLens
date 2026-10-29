using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using PokeBattle.IntegrationTests.TestUtils;

namespace PokeBattle.IntegrationTests;

/// <summary>The Admin role comes from Auth:AdminEmails at login and opens the audit screen.</summary>
[Collection(ApiTests.Name)]
public class AdminTests(ApiFactory factory)
{
    private const string Password = "pikachu-123";
    private const string AuditPath = "/api/v1/admin/audit";

    [Fact]
    public async Task ConfiguredEmail_GetsAdminRoleAndSeesTheLatestPredictions()
    {
        var client = await LoggedIn(ApiFactory.AdminEmail);
        await client.PostAsJsonAsync("/api/v1/battles/predict", new
        {
            target = "rattata",
            area = "kanto-route-1-area",
            method = "walk",
            version = "firered",
            milestone = "pokedex",
            objective = "Catch",
            scenario = "Neutral",
        });

        using var me = JsonDocument.Parse(await client.GetStringAsync("/api/v1/auth/me"));
        Assert.Equal(["Admin"], me.RootElement.GetProperty("roles").EnumerateArray().Select(r => r.GetString()));

        var response = await client.GetAsync($"{AuditPath}?limit=2");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var audit = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var latest = audit.RootElement.EnumerateArray().ToList();
        Assert.Equal(2, latest.Count);
        Assert.Equal("BattlePredictionCompleted", latest[0].GetProperty("event").GetString());
        Assert.Equal(ApiFactory.AdminEmail, latest[0].GetProperty("userEmail").GetString());
        Assert.Equal(("rattata", "Catch"), (latest[0].GetProperty("target").GetString(), latest[0].GetProperty("objective").GetString()));
    }

    [Fact]
    public async Task RegularUser_IsForbidden()
    {
        var client = await LoggedIn($"trainer-{Guid.NewGuid():N}@example.com");

        using var me = JsonDocument.Parse(await client.GetStringAsync("/api/v1/auth/me"));
        Assert.Empty(me.RootElement.GetProperty("roles").EnumerateArray());
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync(AuditPath)).StatusCode);
    }

    [Fact]
    public async Task Anonymous_IsUnauthorized()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateClient().GetAsync(AuditPath)).StatusCode);
    }

    [Fact]
    public async Task LimitOutOfRange_ReturnsValidationProblem()
    {
        var client = await LoggedIn(ApiFactory.AdminEmail);

        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync($"{AuditPath}?limit=0")).StatusCode);
    }

    private async Task<HttpClient> LoggedIn(string email)
    {
        var client = factory.CreateClient();
        // The admin may already exist from another test; registering again just fails with 400.
        await client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = Password });
        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = Password });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        using var body = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body.RootElement.GetProperty("accessToken").GetString());
        return client;
    }
}