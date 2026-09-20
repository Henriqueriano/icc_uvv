using backend.Data;

namespace backend.Scripts;

public static class SeedAdmin
{
    public static Task RunAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        return DatabaseInitializer.SeedAdminAsync(services, cancellationToken);
    }
}
