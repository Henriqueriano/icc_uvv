namespace backend.Contracts.Rdf;

public class RdfImportRequest
{
    public string GraphName { get; set; } = string.Empty;
    public string Format { get; set; } = "text/turtle";
}
