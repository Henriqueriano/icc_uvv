using backend.Infrastructure.Exceptions;
using backend.Options;

namespace backend.Infrastructure.Fuseki;

public class FusekiClient : IFusekiClient
{
    private readonly HttpClient _httpClient;
    private readonly RdfOptions _options;

    public FusekiClient(HttpClient httpClient, RdfOptions options)
    {
        _httpClient = httpClient;
        _options = options;

        if (!Uri.TryCreate(_options.FusekiBaseUrl, UriKind.Absolute, out var baseUri))
        {
            throw new ArgumentException("The Fuseki base URL is invalid.", nameof(options));
        }

        _httpClient.BaseAddress = baseUri;
    }

    public async Task<string> PingAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/$/ping", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new DependencyUnavailableException(
                    $"Fuseki is not available. Status: {(int)response.StatusCode} {response.ReasonPhrase}");
            }

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("The Fuseki ping request timed out.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new DependencyUnavailableException("Unable to reach the Fuseki service.", ex);
        }
    }
}
