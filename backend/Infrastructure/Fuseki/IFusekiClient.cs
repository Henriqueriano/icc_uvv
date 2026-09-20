namespace backend.Infrastructure.Fuseki;

public interface IFusekiClient
{
    Task<string> PingAsync(CancellationToken cancellationToken = default);
}
