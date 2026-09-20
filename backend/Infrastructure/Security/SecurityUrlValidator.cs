using System.Net;
using System.Net.Sockets;
using backend.Options;

namespace backend.Infrastructure.Security;

public static class SecurityUrlValidator
{
    public static bool IsAllowedUrl(string? candidate, SecurityOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return true;
        }

        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var resolvedOptions = options ?? new SecurityOptions();
        var host = uri.Host;
        var isLoopbackOrPrivate = IsLoopbackOrPrivateHost(host);

        if (isLoopbackOrPrivate && !resolvedOptions.AllowLoopbackUrls)
        {
            return false;
        }

        if (resolvedOptions.AllowedHosts.Contains(host, StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        return !isLoopbackOrPrivate;
    }

    public static void ValidateConfiguredUrl(string? candidate, string settingName, SecurityOptions options)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            throw new InvalidOperationException($"Configuration '{settingName}' is required.");
        }

        if (!IsAllowedUrl(candidate, options))
        {
            throw new InvalidOperationException($"Configuration '{settingName}' points to a disallowed external URL.");
        }
    }

    private static bool IsLoopbackOrPrivateHost(string host)
    {
        if (string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (IPAddress.TryParse(host, out var address))
        {
            return IsPrivateOrLoopbackAddress(address);
        }

        try
        {
            var addresses = Dns.GetHostAddresses(host);
            return addresses.Any(IsPrivateOrLoopbackAddress);
        }
        catch
        {
            return true;
        }
    }

    private static bool IsPrivateOrLoopbackAddress(IPAddress address)
    {
        if (IPAddress.IsLoopback(address))
        {
            return true;
        }

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = address.GetAddressBytes();
            return bytes[0] == 10
                || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                || (bytes[0] == 192 && bytes[1] == 168)
                || (bytes[0] == 127);
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            return address.IsIPv6LinkLocal
                || address.IsIPv6SiteLocal
                || address.IsIPv6Multicast
                || address.IsIPv6Teredo
                || address.IsIPv6UniqueLocal;
        }

        return false;
    }
}
