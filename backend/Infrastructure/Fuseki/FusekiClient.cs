using backend.Infrastructure.Exceptions;
using backend.Options;
using System.Net.Http.Headers;
using System.Text;

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
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(
            $"{_options.FusekiUsername}:{_options.FusekiPassword}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
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
                var values = new List<KeyValuePair<string, string>> { new("query", query) };
                if (!string.IsNullOrWhiteSpace(defaultGraph))
                {
                    values.Add(new("default-graph-uri", defaultGraph));
                }

                using var request = new HttpRequestMessage(HttpMethod.Post, $"/{_options.FusekiDataset}/query")
                {
                    Content = new FormUrlEncodedContent(values)
                };
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/sparql-results+json"));

                var response = await _httpClient.SendAsync(request, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    throw new SparqlException(
                        $"Fuseki rejected the SPARQL query. Status: {(int)response.StatusCode} {response.ReasonPhrase}. Details: {body}");
                }

                return body;
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException("The Fuseki query timed out.", ex);
            }
            catch (HttpRequestException ex)
            {
                throw new DependencyUnavailableException("Unable to reach the Fuseki service.", ex);
        }
    }
}
