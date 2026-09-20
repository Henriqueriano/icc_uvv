namespace backend.Contracts.Rdf;

public class GraphContentResult
{
    public string Name { get; set; } = string.Empty;
    public string Format { get; set; } = "text/turtle";
    public string Content { get; set; } = string.Empty;
}
