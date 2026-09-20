using backend.Infrastructure.Exceptions;
using backend.Infrastructure.Qlever;
using backend.Options;

namespace backend.Infrastructure.Health;

public class RdfHealthCheck : IRdfHealthCheck
{
    private readonly IQleverClient _qleverClient;
    private readonly RdfOptions _options;

    public RdfHealthCheck(IQleverClient qleverClient, RdfOptions options)
    {
        _qleverClient = qleverClient;
        _options = options;
    }

    public async Task<bool> IsReadyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _qleverClient.PingAsync(cancellationToken);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
