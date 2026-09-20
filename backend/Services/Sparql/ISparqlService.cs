using backend.Contracts.Sparql;

namespace backend.Services.Sparql;

public interface ISparqlService
{
    Task<string> ExecuteQueryAsync(SparqlQueryRequest request, CancellationToken cancellationToken = default);
    Task<string> ValidateAsync(string query, CancellationToken cancellationToken = default);
}
