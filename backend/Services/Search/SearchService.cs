using backend.Contracts.Search;
using backend.Infrastructure.Exceptions;
using backend.Infrastructure.Qlever;

namespace backend.Services.Search;

public class SearchService : ISearchService
{
    private readonly IQleverClient _qleverClient;

    public SearchService(IQleverClient qleverClient)
    {
        _qleverClient = qleverClient;
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

        var terms = request.Text.Trim();
        var escapedTerms = EscapeSparqlString(terms);
        var query = $@"
            SELECT ?subject ?predicate ?object
            WHERE {{
              FILTER(
                REGEX(STR(?subject), ""{escapedTerms}"", ""i"")
                || REGEX(STR(?predicate), ""{escapedTerms}"", ""i"")
                || REGEX(STR(?object), ""{escapedTerms}"", ""i"")
              )
              ?subject ?predicate ?object .
            }}
            LIMIT {request.PageSize}
        ";

        try
        {
            return await _qleverClient.ExecuteQueryAsync(query, request.Graph, cancellationToken);
        }
        catch (Exception ex) when (ex is SparqlException or TimeoutException or DependencyUnavailableException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new SparqlException("The free search could not be completed.", ex);
        }
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
