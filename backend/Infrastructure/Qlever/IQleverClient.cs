namespace backend.Infrastructure.Qlever;

public interface IQleverClient
{
    Task<string> PingAsync(CancellationToken cancellationToken = default);
    Task<string> ExecuteQueryAsync(string query, string? defaultGraph = null, CancellationToken cancellationToken = default);
    Task UploadAsync(Stream content, string contentType, string? graphName = null, CancellationToken cancellationToken = default);
}
