using backend.Infrastructure.Exceptions;
using backend.Infrastructure.Fuseki;
using backend.Infrastructure.Qlever;
using backend.Options;

namespace backend.Infrastructure.Health;

public class RdfHealthCheck : IRdfHealthCheck
{
    private readonly IFusekiClient _fusekiClient;
    private readonly IQleverClient _qleverClient;
    private readonly RdfOptions _options;

    public RdfHealthCheck(IFusekiClient fusekiClient, IQleverClient qleverClient, RdfOptions options)
    {
        _fusekiClient = fusekiClient;
        _qleverClient = qleverClient;
        _options = options;
    }

    public async Task<bool> IsReadyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _fusekiClient.PingAsync(cancellationToken);
            await _qleverClient.ExecuteQueryAsync(
                "SELECT * WHERE { ?s ?p ?o } LIMIT 1",
                _options.QleverIndex,
                cancellationToken);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
