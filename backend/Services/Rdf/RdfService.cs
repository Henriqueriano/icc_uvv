using backend.Infrastructure.Qlever;
using backend.Options;

namespace backend.Services.Rdf;

public class RdfService : IRdfService
{
    private readonly IQleverClient _qleverClient;
    private readonly RdfOptions _options;

    public RdfService(IQleverClient qleverClient, RdfOptions options)
    {
        _qleverClient = qleverClient;
        _options = options;
    }

    public async Task<string> GetConfigurationSummaryAsync(CancellationToken cancellationToken = default)
    {
        var qleverStatus = await _qleverClient.PingAsync(cancellationToken);
        return $"QLever: {qleverStatus.Substring(0, Math.Min(qleverStatus.Length, 120))}";
    }
}
