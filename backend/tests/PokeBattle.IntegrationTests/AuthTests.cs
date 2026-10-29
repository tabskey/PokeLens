using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using PokeBattle.Application.Auditing;
using PokeBattle.IntegrationTests.TestUtils;

namespace PokeBattle.IntegrationTests;

/// <summary>Optional accounts (register, login, JWT) and the audit log of predictions.</summary>
[Collection(ApiTests.Name)]
public class AuthTests(ApiFactory factory)
{
    private const string Password = "pikachu-123";

    [Fact]
    public async Task RegisterAndLogin_TokenAuthenticatesTheUser()
    {
        var email = NewEmail();
        var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.Created, (await Register(client, email, Password)).StatusCode);
        var token = await Login(client, email);
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var me = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        using var body = JsonDocument.Parse(await me.Content.ReadAsStringAsync());
        Assert.Equal(email, body.RootElement.GetProperty("email").GetString());
    }

    [Fact]
    public async Task Me_WithoutToken_ReturnsUnauthorized()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateClient().GetAsync("/api/v1/auth/me")).StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReportsEmail()
    {
        var email = NewEmail();
        var client = factory.CreateClient();
        await Register(client, email, Password);

        var errors = await Errors(await Register(client, email, Password));

        Assert.Equal(["Já existe uma conta com esse e-mail."], errors["email"]);
    }

    [Fact]
    public async Task Register_ShortPassword_ReportsPassword()
    {
        var errors = await Errors(await Register(factory.CreateClient(), NewEmail(), "abc"));

        Assert.Equal(["A senha precisa ter pelo menos 8 caracteres."], errors["password"]);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var email = NewEmail();
        var client = factory.CreateClient();
        await Register(client, email, Password);

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "wrong-password" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Predict_LoggedIn_AuditsRequestAndCompletionWithTheUser()
    {
        var email = NewEmail();
        var client = factory.CreateClient();
        await Register(client, email, Password);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await Login(client, email));
        var userId = await factory.QueryAsync(db => db.Users.Where(u => u.Email == email).Select(u => u.Id).SingleAsync());

        Assert.Equal(HttpStatusCode.OK, (await Predict(client)).StatusCode);

        var entries = await factory.QueryAsync(db => db.AuditLog.Where(e => e.UserId == userId).OrderBy(e => e.Id).ToListAsync());
        Assert.Equal([AuditEvents.BattlePredictionRequested, AuditEvents.BattlePredictionCompleted], entries.Select(e => e.Event));
        Assert.All(entries, e => Assert.Equal(("pidgey", "pokedex"), (e.Target, e.Milestone)));
        Assert.Empty(entries[0].Candidates);
        Assert.NotEmpty(entries[1].Candidates);
    }

    [Fact]
    public async Task Predict_Anonymous_IsAuditedWithoutUser()
    {
        var before = await factory.QueryAsync(db => db.AuditLog.CountAsync(e => e.UserId == null));

        Assert.Equal(HttpStatusCode.OK, (await Predict(factory.CreateClient())).StatusCode);

        Assert.Equal(before + 2, await factory.QueryAsync(db => db.AuditLog.CountAsync(e => e.UserId == null)));
    }

    private static string NewEmail() => $"trainer-{Guid.NewGuid():N}@example.com";

    private static Task<HttpResponseMessage> Register(HttpClient client, string email, string password)
        => client.PostAsJsonAsync("/api/v1/auth/register", new { email, password });

    private static async Task<string> Login(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = Password });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("accessToken").GetString()!;
    }

    private static Task<HttpResponseMessage> Predict(HttpClient client)
        => client.PostAsJsonAsync("/api/v1/battles/predict", new
        {
            target = "pidgey",
            area = "kanto-route-1-area",
            method = "walk",
            version = "firered",
            milestone = "pokedex",
            objective = "Defeat",
            scenario = "Neutral",
        });

    private static async Task<Dictionary<string, string[]>> Errors(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("errors").Deserialize<Dictionary<string, string[]>>()!;
    }
}