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

        _httpClient.BaseAddress = new Uri(_options.FusekiBaseUrl);
    }

    public async Task<string> PingAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("/$/ping", cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
