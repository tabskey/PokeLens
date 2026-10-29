using System.Net;

using PokeBattle.IntegrationTests.TestUtils;

namespace PokeBattle.IntegrationTests;

[Collection(ApiTests.Name)]
public class PingAndHealthTests(ApiFactory factory)
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