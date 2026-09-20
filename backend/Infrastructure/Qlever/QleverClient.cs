using backend.Infrastructure.Exceptions;
using backend.Options;
using System.Net.Http.Headers;

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

    public async Task<string> ExecuteQueryAsync(
        string query,
        string? defaultGraph = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("A SPARQL query is required.", nameof(query));
        }

        try
        {
            var payload = new List<KeyValuePair<string, string>> { new("query", query) };
            if (!string.IsNullOrWhiteSpace(defaultGraph)
                && !string.Equals(defaultGraph, _options.QleverIndex, StringComparison.OrdinalIgnoreCase))
            {
                payload.Add(new("default-graph-uri", defaultGraph));
            }
            using var request = new HttpRequestMessage(HttpMethod.Post, "/sparql")
            {
                Content = new FormUrlEncodedContent(payload)
            };

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new SparqlException(
                    $"QLever rejected the SPARQL query. Status: {(int)response.StatusCode} {response.ReasonPhrase}. Details: {body}");
            }

            return body;
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

    public Task<string> PingAsync(CancellationToken cancellationToken = default)
        => ExecuteQueryAsync("ASK WHERE { ?s ?p ?o }", _options.QleverIndex, cancellationToken);

    public async Task UploadAsync(
        Stream content,
        string contentType,
        string? graphName = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new ArgumentException("The RDF content type is required.", nameof(contentType));
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "/update")
        {
            Content = new StreamContent(content)
        };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        if (!string.IsNullOrWhiteSpace(graphName))
        {
            request.Headers.Add("X-QLever-Graph", graphName);
        }

        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new SparqlException(
                    $"QLever rejected the RDF update. Status: {(int)response.StatusCode} {response.ReasonPhrase}. Details: {body}");
            }
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("The QLever RDF update timed out.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new DependencyUnavailableException("Unable to reach the QLever service for RDF update.", ex);
        }
    }
}
