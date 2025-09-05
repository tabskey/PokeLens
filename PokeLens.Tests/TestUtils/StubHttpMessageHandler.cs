using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PokeLens.Tests.TestUtils;

public class StubHttpMessageHandler : HttpMessageHandler
{
	private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

	public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
	{
		_responseFactory = responseFactory;
	}

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		var response = _responseFactory(request);
		return Task.FromResult(response);
	}

	public static HttpClient CreateClient(Func<HttpRequestMessage, HttpResponseMessage> responseFactory, Uri? baseAddress = null)
	{
		var handler = new StubHttpMessageHandler(responseFactory);
		var client = new HttpClient(handler);
		if (baseAddress != null)
		{
			client.BaseAddress = baseAddress;
		}
		return client;
	}
}




