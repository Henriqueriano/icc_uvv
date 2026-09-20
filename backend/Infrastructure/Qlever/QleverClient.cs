using backend.Options;

namespace backend.Infrastructure.Qlever;

public class QleverClient : IQleverClient
{
    private readonly HttpClient _httpClient;
    private readonly RdfOptions _options;

    public QleverClient(HttpClient httpClient, RdfOptions options)
    {
        _httpClient = httpClient;
        _options = options;

        _httpClient.BaseAddress = new Uri(_options.QleverBaseUrl);
    }

    public async Task<string> ExecuteQueryAsync(string query, string? defaultGraph = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("A SPARQL query is required.", nameof(query));
        }

        var payload = new List<KeyValuePair<string, string>>
        {
            new("query", query),
            new("default-graph-uri", defaultGraph ?? _options.QleverIndex)
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "/sparql")
        {
            Content = new FormUrlEncodedContent(payload)
        };

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
