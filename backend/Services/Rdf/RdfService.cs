using backend.Infrastructure.Fuseki;
using backend.Infrastructure.Qlever;
using backend.Options;

namespace backend.Services.Rdf;

public class RdfService : IRdfService
{
    private readonly IFusekiClient _fusekiClient;
    private readonly IQleverClient _qleverClient;
    private readonly RdfOptions _options;

    public RdfService(IFusekiClient fusekiClient, IQleverClient qleverClient, RdfOptions options)
    {
        _fusekiClient = fusekiClient;
        _qleverClient = qleverClient;
        _options = options;
    }

    public async Task<string> GetConfigurationSummaryAsync(CancellationToken cancellationToken = default)
    {
        var fusekiStatus = await _fusekiClient.PingAsync(cancellationToken);
        var qleverStatus = await _qleverClient.ExecuteQueryAsync(
            "SELECT * WHERE { ?s ?p ?o } LIMIT 1",
            _options.QleverIndex,
            cancellationToken);

        return $"Fuseki: {fusekiStatus}; QLever: {qleverStatus.Substring(0, Math.Min(qleverStatus.Length, 120))}";
    }
}
