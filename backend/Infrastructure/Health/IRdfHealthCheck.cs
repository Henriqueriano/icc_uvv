namespace backend.Infrastructure.Health;

public interface IRdfHealthCheck
{
    Task<bool> IsReadyAsync(CancellationToken cancellationToken = default);
}
