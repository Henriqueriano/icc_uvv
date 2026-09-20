using System.ComponentModel.DataAnnotations;

namespace backend.Options;

public class SecurityOptions
{
    public const string SectionName = "Security";

    [Range(1, 10000)]
    public int RateLimitPerMinute { get; set; } = 120;

    [Range(1, 5000)]
    public int AuthRateLimitPerMinute { get; set; } = 10;

    [Range(1, 5000)]
    public int SearchRateLimitPerMinute { get; set; } = 30;

    public bool AllowLoopbackUrls { get; set; } = false;

    public string[] AllowedHosts { get; set; } = ["qlever"];
}
