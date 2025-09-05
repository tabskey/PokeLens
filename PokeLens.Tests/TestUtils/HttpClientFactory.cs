using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace PokeLens.Tests.TestUtils;

public static class HttpClientFactory
{
	public static HttpClient CreateJsonClient(Func<HttpRequestMessage, HttpResponseMessage> responder, string? baseAddress = null)
	{
		var client = StubHttpMessageHandler.CreateClient(responder, baseAddress != null ? new Uri(baseAddress) : null);
		client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		client.DefaultRequestHeaders.UserAgent.ParseAdd("PokeLens.Tests");
		return client;
	}
}




