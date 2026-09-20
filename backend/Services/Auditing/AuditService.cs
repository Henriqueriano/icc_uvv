using System.Text.RegularExpressions;

namespace backend.Services.Auditing;

public class AuditService(ILogger<AuditService> logger) : IAuditService
{
    public Task AuditAsync(string action, string resource, string? actor, string? details, bool success, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            timestamp = DateTimeOffset.UtcNow,
            action,
            resource,
            actor = Redact(actor),
            details = Sanitize(details),
            success
        };

        logger.LogInformation("RDF audit event {AuditEvent}", payload);
        return Task.CompletedTask;
    }

    private static string? Redact(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var normalized = value.Trim();
        return normalized.Length <= 2 ? "***" : normalized[..2] + "***";
    }

    private static string? Sanitize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var masked = Regex.Replace(value, "(?i)(password|secret|token|authorization|jwt|api[_-]?key)\\s*[:=]\\s*[^,;\\s]+", "$1=[REDACTED]");
        masked = Regex.Replace(masked, "(?i)(Bearer\\s+)[A-Za-z0-9._-]+", "$1[REDACTED]");
        return masked;
    }
}
