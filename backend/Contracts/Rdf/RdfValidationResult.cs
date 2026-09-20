namespace backend.Contracts.Rdf;

public class RdfValidationResult
{
    public bool IsValid { get; set; }
    public int TripleCount { get; set; }
    public string Format { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
