using System.Diagnostics;
using backend.Contracts.Search;
using backend.Infrastructure.Exceptions;
using backend.Infrastructure.Ollama;
using backend.Infrastructure.Qlever;
using backend.Services.Statistics;
using System.Text.Json.Nodes;

namespace backend.Services.Search;

public class SearchService : ISearchService
{
    private readonly IQleverClient _qleverClient;
    private readonly IOllamaClient _ollamaClient;
    private readonly IOperationalMetricsService _metricsService;

    public SearchService(IQleverClient qleverClient, IOllamaClient ollamaClient, IOperationalMetricsService? metricsService = null)
    {
        _qleverClient = qleverClient;
        _ollamaClient = ollamaClient;
        _metricsService = metricsService ?? new OperationalMetricsService();
    }

    public async Task<string> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Text))
        {
            throw new ArgumentException("The search text is required.", nameof(request));
        }

        if (request.Page < 1 || request.PageSize < 1 || request.PageSize > 200)
        {
            throw new ArgumentException("Page and page size values must be valid.", nameof(request));
        }

        var stopwatch = Stopwatch.StartNew();
        var success = false;
        var query = string.Empty;
        try
        {
            string? qleverError = null;
            for (var attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    query = await GenerateQueryAsync(request.Text.Trim(), request.PageSize, qleverError, cancellationToken);
                    var qleverResponse = await _qleverClient.ExecuteQueryAsync(query, request.Graph, cancellationToken);
                    success = true;
                    return AddGeneratedQuery(qleverResponse, query);
                }
                catch (SparqlException ex) when (attempt < 2)
                {
                    qleverError = ex.Message;
                }
            }

            throw new SparqlException("QLever rejected the generated SPARQL query after multiple repair attempts.");
        }

        catch (Exception ex) when (ex is SparqlException or TimeoutException or DependencyUnavailableException)
        {
            if (ex is not SparqlException || !ex.Data.Contains("ollamaResponse"))
            {
                if (!string.IsNullOrWhiteSpace(query))
                {
                    ex.Data["ollamaResponse"] = query;
                }
            }
            throw;
        }
        catch (Exception ex)
        {
            throw new SparqlException("The free search could not be completed.", ex);
        }
        finally
        {
            stopwatch.Stop();
            _metricsService.RecordFreeSearch(stopwatch.ElapsedMilliseconds, success);
        }
    }

    private static string AddGeneratedQuery(string qleverResponse, string generatedQuery)
    {
        var response = JsonNode.Parse(qleverResponse)?.AsObject()
            ?? throw new SparqlException("QLever returned an invalid JSON response.");
        response["ollamaResponse"] = generatedQuery;
        return response.ToJsonString();
    }

    private async Task<string> GenerateQueryAsync(
        string text,
        int pageSize,
        string? qleverError,
        CancellationToken cancellationToken)
    {
        var prompt = $"""
            Convert the user's request into one read-only SPARQL 1.1 query for an RDF knowledge graph.
            Return only the SPARQL query, without Markdown fences or explanations.
            The query must use variables named ?subject, ?predicate, and ?object when returning triples.
            Never generate INSERT, DELETE, LOAD, CLEAR, DROP, CREATE, or other updates.
            Always include LIMIT {pageSize} unless the query is an aggregate.
            User request: {text}
            """;
        if (!string.IsNullOrWhiteSpace(qleverError))
        {
            prompt += $"""

                QLever rejected the previous query with this error:
                {qleverError}
                Repair the query for QLever and return only the corrected SPARQL.
                """;
        }

        var generated = await _ollamaClient.GenerateAsync(prompt, cancellationToken);
        var query = generated
            .Replace("```sparql", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("```", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim();
        if (!IsReadOnlyQuery(query))
        {
            throw new SparqlException("Ollama returned an invalid or non-read-only SPARQL query.");
        }

        return query;
    }

    private static bool IsReadOnlyQuery(string query)
    {
        return !string.IsNullOrWhiteSpace(query)
            && !query.Contains("INSERT", StringComparison.OrdinalIgnoreCase)
            && !query.Contains("DELETE", StringComparison.OrdinalIgnoreCase)
            && !query.Contains("UPDATE", StringComparison.OrdinalIgnoreCase)
            && !query.Contains("LOAD", StringComparison.OrdinalIgnoreCase)
            && !query.Contains("CLEAR", StringComparison.OrdinalIgnoreCase)
            && !query.Contains("DROP", StringComparison.OrdinalIgnoreCase)
            && !query.Contains("CREATE", StringComparison.OrdinalIgnoreCase)
            && (query.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase)
                || query.StartsWith("ASK", StringComparison.OrdinalIgnoreCase)
                || query.StartsWith("CONSTRUCT", StringComparison.OrdinalIgnoreCase)
                || query.StartsWith("DESCRIBE", StringComparison.OrdinalIgnoreCase)
                || query.StartsWith("PREFIX", StringComparison.OrdinalIgnoreCase)
                || query.StartsWith("BASE", StringComparison.OrdinalIgnoreCase));
    }

    public async Task<string> SuggestionsAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("The text for suggestions is required.", nameof(text));
        }

        var escapedText = EscapeSparqlString(text.Trim());
        var query = $@"
            PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
            SELECT DISTINCT ?label
            WHERE {{
              ?s rdfs:label ?label .
              FILTER(REGEX(STR(?label), ""{escapedText}"", ""i""))
            }}
            LIMIT 10
        ";

        try
        {
            return await _qleverClient.ExecuteQueryAsync(query, null, cancellationToken);
        }
        catch (Exception ex) when (ex is SparqlException or TimeoutException or DependencyUnavailableException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new SparqlException("Suggestions could not be generated.", ex);
        }
    }

    private static string EscapeSparqlString(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n");
    }
}
