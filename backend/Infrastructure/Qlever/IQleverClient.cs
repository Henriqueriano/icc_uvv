namespace backend.Infrastructure.Qlever;

public interface IQleverClient
{
    Task<string> ExecuteQueryAsync(string query, string? defaultGraph = null, CancellationToken cancellationToken = default);
}
