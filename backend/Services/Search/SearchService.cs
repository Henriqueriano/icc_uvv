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
        var query = $@"
            SELECT ?subject ?predicate ?object
            WHERE {{
              {{ ?subject ?predicate ?object . }}
              FILTER(
                CONTAINS(LCASE(STR(?subject)), LCASE('{EscapeSqlLike(terms)}'))
                || CONTAINS(LCASE(STR(?predicate)), LCASE('{EscapeSqlLike(terms)}'))
                || CONTAINS(LCASE(STR(?object)), LCASE('{EscapeSqlLike(terms)}'))
              )
            }}
            LIMIT {request.PageSize}
        ";

        try
        {
            return await _qleverClient.ExecuteQueryAsync(query, request.Graph, cancellationToken);
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

        var query = $@"
            SELECT DISTINCT ?label
            WHERE {{
              ?s rdfs:label ?label .
              FILTER(CONTAINS(LCASE(STR(?label)), LCASE('{EscapeSqlLike(text.Trim())}')))
            }}
            LIMIT 10
        ";

        try
        {
            return await _qleverClient.ExecuteQueryAsync(query, null, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new SparqlException("Suggestions could not be generated.", ex);
        }
    }

    private static string EscapeSqlLike(string value)
    {
        return value.Replace("'", "\\'");
    }
}
