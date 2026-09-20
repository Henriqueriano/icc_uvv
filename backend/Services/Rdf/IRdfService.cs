namespace backend.Services.Rdf;

public interface IRdfService
{
    Task<string> GetConfigurationSummaryAsync(CancellationToken cancellationToken = default);
}
