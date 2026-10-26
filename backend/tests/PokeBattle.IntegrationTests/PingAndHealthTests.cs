using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;

namespace PokeBattle.IntegrationTests;

public class PingAndHealthTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Theory]
    [InlineData("/api/v1/ping")]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task Endpoint_ReturnsOk(string path)
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}