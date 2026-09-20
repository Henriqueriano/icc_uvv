using backend.Contracts.Sparql;
using backend.Infrastructure.Exceptions;
using backend.Infrastructure.Qlever;

namespace backend.Services.Sparql;

public class SparqlService : ISparqlService
{
    private readonly IQleverClient _qleverClient;

    public SparqlService(IQleverClient qleverClient)
    {
        _qleverClient = qleverClient;
    }

    public async Task<string> ExecuteQueryAsync(SparqlQueryRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Query))
        {
            throw new ArgumentException("The SPARQL query is required.", nameof(request));
        }

        var normalizedQuery = request.Query.Trim();
        var defaultGraph = request.DefaultGraph;

        if (normalizedQuery.Contains("INSERT", StringComparison.OrdinalIgnoreCase)
            || normalizedQuery.Contains("DELETE", StringComparison.OrdinalIgnoreCase)
            || normalizedQuery.Contains("UPDATE", StringComparison.OrdinalIgnoreCase))
        {
            throw new SparqlException("Write operations are not allowed through the read-only QLever query endpoint.");
        }

        try
        {
            return await _qleverClient.ExecuteQueryAsync(normalizedQuery, defaultGraph, cancellationToken);
        }
        catch (Exception ex) when (ex is TimeoutException or DependencyUnavailableException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new SparqlException("The SPARQL query could not be executed.", ex);
        }
    }

    public Task<string> ValidateAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("The SPARQL query is required.", nameof(query));
        }

        var sanitized = query.Trim();
        if (sanitized.Length > 10000)
        {
            throw new ArgumentException("The SPARQL query exceeds the allowed maximum length.", nameof(query));
        }

        return Task.FromResult($"SPARQL query validated: {sanitized.Substring(0, Math.Min(sanitized.Length, 120))}");
    }
}
