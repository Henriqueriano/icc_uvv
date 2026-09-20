using backend.Contracts.Rdf;
using backend.Infrastructure.Exceptions;
using backend.Infrastructure.Qlever;
using backend.Options;

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

    public Task<IReadOnlyList<GraphMetadataDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var graphs = new List<GraphMetadataDto>
        {
            new()
            {
                Name = _options.FusekiDataset,
                Status = "configured",
                IsDefault = true,
                TripleCount = 0,
                LastUpdatedUtc = DateTimeOffset.UtcNow
            },
            new()
            {
                Name = _options.QleverIndex,
                Status = "configured",
                IsDefault = false,
                TripleCount = 0,
                LastUpdatedUtc = DateTimeOffset.UtcNow
            }
        };

        return Task.FromResult<IReadOnlyList<GraphMetadataDto>>(graphs);
    }

    public Task<GraphMetadataDto?> GetByNameAsync(string graphName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(graphName))
        {
            throw new ArgumentException("The graph name is required.", nameof(graphName));
        }

        var graph = new GraphMetadataDto
        {
            Name = graphName,
            Status = "configured",
            IsDefault = graphName == _options.FusekiDataset || graphName == _options.QleverIndex,
            TripleCount = 0,
            LastUpdatedUtc = DateTimeOffset.UtcNow
        };

        return Task.FromResult<GraphMetadataDto?>(graph);
    }

    public Task<GraphStatisticsDto> GetStatisticsAsync(string graphName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(graphName))
        {
            throw new ArgumentException("The graph name is required.", nameof(graphName));
        }

        var stats = new GraphStatisticsDto
        {
            Name = graphName,
            TripleCount = 0,
            QueryCount = 0,
            LastUpdatedUtc = DateTimeOffset.UtcNow
        };

        return Task.FromResult(stats);
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
