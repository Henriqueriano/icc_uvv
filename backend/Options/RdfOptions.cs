using System.ComponentModel.DataAnnotations;

namespace backend.Options;

public class RdfOptions
{
    public const string SectionName = "Rdf";

    [Required]
    [Url]
    public string FusekiBaseUrl { get; set; } = string.Empty;

    [Required]
    public string FusekiDataset { get; set; } = string.Empty;

    [Required]
    public string FusekiUsername { get; set; } = string.Empty;

    [Required]
    public string FusekiPassword { get; set; } = string.Empty;

    [Required]
    [Url]
    public string QleverBaseUrl { get; set; } = string.Empty;

    [Required]
    public string QleverIndex { get; set; } = string.Empty;

    [Range(1, 600)]
    public int QueryTimeoutSeconds { get; set; } = 30;

    [Range(1, 100000)]
    public int MaxResults { get; set; } = 1000;

    [Range(1, 104857600)]
    public long MaxUploadBytes { get; set; } = 50 * 1024 * 1024;
}
