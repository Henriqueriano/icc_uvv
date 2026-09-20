using System.ComponentModel.DataAnnotations;

namespace backend.Options;

public class OllamaOptions
{
    public const string SectionName = "Ollama";

    [Required]
    [Url]
    public string BaseUrl { get; set; } = string.Empty;

    [Required]
    public string Model { get; set; } = "llama3.2";

    [Range(1, 600)]
    public int TimeoutSeconds { get; set; } = 60;
}
