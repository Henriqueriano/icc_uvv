namespace backend.Services.Auditing;

public interface IAuditService
{
    Task AuditAsync(string action, string resource, string? actor, string? details, bool success, CancellationToken cancellationToken = default);
}
