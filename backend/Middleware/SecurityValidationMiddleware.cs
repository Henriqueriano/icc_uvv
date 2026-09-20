using backend.Infrastructure.Security;
using backend.Options;

namespace backend.Middleware;

public class SecurityValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SecurityOptions _options;

    public SecurityValidationMiddleware(RequestDelegate next, SecurityOptions options)
    {
        _next = next;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        foreach (var candidate in GetCandidateUrls(context))
        {
            if (!SecurityUrlValidator.IsAllowedUrl(candidate, _options))
            {
                throw new InvalidOperationException("External URLs are not permitted in request metadata.");
            }
        }

        await _next(context);
    }

    private static IEnumerable<string> GetCandidateUrls(HttpContext context)
    {
        var values = new List<string>();

        foreach (var key in new[] { "url", "uri", "sourceUrl", "targetUrl", "redirectUrl" })
        {
            if (context.Request.Query.TryGetValue(key, out var queryValue))
            {
                values.AddRange(queryValue
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Select(v => v ?? string.Empty));
            }
        }

        foreach (var headerName in new[] { "X-Forwarded-Uri", "Referer", "Origin", "X-Redirect-Url" })
        {
            if (context.Request.Headers.TryGetValue(headerName, out var headerValue))
            {
                values.AddRange(headerValue
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Select(v => v ?? string.Empty));
            }
        }

        return values.Distinct(StringComparer.OrdinalIgnoreCase);
    }
}
