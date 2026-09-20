using backend.Infrastructure.Exceptions;
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

        if (!Uri.TryCreate(_options.QleverBaseUrl, UriKind.Absolute, out var baseUri))
        {
            throw new ArgumentException("The QLever base URL is invalid.", nameof(options));
        }

        _httpClient.BaseAddress = baseUri;
    }

    public async Task<string> ExecuteQueryAsync(string query, string? defaultGraph = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("A SPARQL query is required.", nameof(query));
        }

        var graphName = string.IsNullOrWhiteSpace(defaultGraph) ? _options.QleverIndex : defaultGraph;

        if (string.IsNullOrWhiteSpace(graphName))
        {
            throw new ArgumentException("A default graph for QLever is required.", nameof(defaultGraph));
        }

        try
        {
            var payload = new List<KeyValuePair<string, string>>
            {
                new("query", query),
                new("default-graph-uri", graphName)
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "/sparql")
            {
                Content = new FormUrlEncodedContent(payload)
            };

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new SparqlException(
                    $"QLever rejected the SPARQL query. Status: {(int)response.StatusCode} {response.ReasonPhrase}. Details: {errorBody}");
            }

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("The QLever query timed out.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new DependencyUnavailableException("Unable to reach the QLever service.", ex);
        }
    }
}
