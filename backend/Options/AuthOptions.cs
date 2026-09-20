using System.ComponentModel.DataAnnotations;

namespace backend.Options;

public class AuthOptions
{
    public const string SectionName = "Auth";

    [Required]
    public string ConnectionStringName { get; set; } = "DefaultConnection";

    [Required]
    public string JwtIssuer { get; set; } = "icc-uvv";

    [Required]
    public string JwtAudience { get; set; } = "icc-uvv-api";

    [Required]
    [MinLength(32)]
    public string JwtKey { get; set; } = "development-jwt-signing-key-icc-uvv-123456";

    [Required]
    public string DefaultUsername { get; set; } = "rdf-admin";

    [Required]
    public string DefaultPassword { get; set; } = "ChangeMe123!";

    [Range(5, 10080)]
    public int TokenExpirationMinutes { get; set; } = 60;
}
