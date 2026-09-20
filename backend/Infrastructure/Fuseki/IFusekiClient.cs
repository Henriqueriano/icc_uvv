namespace backend.Infrastructure.Fuseki;

public interface IFusekiClient
{
    Task<string> PingAsync(CancellationToken cancellationToken = default);
    Task<string> ExecuteQueryAsync(string query, string? defaultGraph = null, CancellationToken cancellationToken = default);
}
