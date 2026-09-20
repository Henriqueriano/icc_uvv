using backend.Contracts.Rdf;
using backend.Infrastructure.Exceptions;
using backend.Infrastructure.Qlever;
using backend.Options;
using System.Text.Json;

namespace backend.Services.Graphs;

public class GraphService : IGraphService
{
    private readonly IQleverClient _qleverClient;
    private readonly RdfOptions _options;

    public GraphService(IQleverClient qleverClient, RdfOptions options)
    {
        _qleverClient = qleverClient;
        _options = options;
    }

    public async Task<IReadOnlyList<GraphMetadataDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var response = await _qleverClient.ExecuteQueryAsync(
            "SELECT ?graph (COUNT(*) AS ?triples) WHERE { GRAPH ?graph { ?s ?p ?o } } GROUP BY ?graph",
            cancellationToken: cancellationToken);
        using var document = JsonDocument.Parse(response);
        return document.RootElement.GetProperty("results").GetProperty("bindings")
            .EnumerateArray()
            .Select(binding => new GraphMetadataDto
            {
                Name = binding.GetProperty("graph").GetProperty("value").GetString()!,
                Status = "available",
                IsDefault = false,
                TripleCount = long.Parse(binding.GetProperty("triples").GetProperty("value").GetString()!),
                LastUpdatedUtc = DateTimeOffset.UtcNow
            }).ToArray();
    }

    public async Task<GraphMetadataDto?> GetByNameAsync(string graphName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(graphName))
        {
            throw new ArgumentException("The graph name is required.", nameof(graphName));
        }

        var graphs = await ListAsync(cancellationToken);
        return graphs.FirstOrDefault(graph => graph.Name == graphName);
    }

    public async Task<GraphStatisticsDto> GetStatisticsAsync(string graphName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(graphName))
        {
            throw new ArgumentException("The graph name is required.", nameof(graphName));
        }

        var graphs = await ListAsync(cancellationToken);
        var graph = graphs.FirstOrDefault(item => item.Name == graphName)
            ?? throw new KeyNotFoundException($"Graph '{graphName}' was not found.");
        return new GraphStatisticsDto
        {
            Name = graph.Name,
            TripleCount = graph.TripleCount,
            QueryCount = 0,
            LastUpdatedUtc = graph.LastUpdatedUtc
        };
    }

    public async Task<GraphContentResult> GetContentAsync(string graphName, string? format = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(graphName))
        {
            throw new ArgumentException("The graph name is required.", nameof(graphName));
        }

        var normalizedFormat = (format ?? "text/turtle").Trim();
        var query = $@"CONSTRUCT {{ ?s ?p ?o }} WHERE {{ GRAPH <{graphName}> {{ ?s ?p ?o }} }} LIMIT 20";

        try
        {
            var content = await _qleverClient.ExecuteQueryAsync(query, graphName, cancellationToken);
            return new GraphContentResult
            {
                Name = graphName,
                Format = normalizedFormat,
                Content = content
            };
        }
        catch (Exception ex)
        {
            throw new DependencyUnavailableException($"Unable to retrieve graph content for '{graphName}'.", ex);
        }
    }
}
