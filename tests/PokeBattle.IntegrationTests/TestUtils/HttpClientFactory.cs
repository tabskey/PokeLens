using System.Net.Http.Headers;

namespace PokeBattle.IntegrationTests.TestUtils;

public static class HttpClientFactory
{
    public static HttpClient CreateJsonClient(Func<HttpRequestMessage, HttpResponseMessage> responder, string? baseAddress = null)
    {
        var client = StubHttpMessageHandler.CreateClient(responder, baseAddress != null ? new Uri(baseAddress) : null);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.UserAgent.ParseAdd("PokeBattle.IntegrationTests");
        return client;
    }
}