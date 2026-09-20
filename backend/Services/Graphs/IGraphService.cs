using backend.Contracts.Rdf;

namespace backend.Services.Graphs;

public interface IGraphService
{
    Task<IReadOnlyList<GraphMetadataDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<GraphMetadataDto?> GetByNameAsync(string graphName, CancellationToken cancellationToken = default);
    Task<GraphStatisticsDto> GetStatisticsAsync(string graphName, CancellationToken cancellationToken = default);
    Task<GraphContentResult> GetContentAsync(string graphName, string? format = null, CancellationToken cancellationToken = default);
}
