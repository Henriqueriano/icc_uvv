namespace backend.Contracts.Sparql;

public class SparqlQueryRequest
{
    public string Query { get; set; } = string.Empty;
    public string? DefaultGraph { get; set; }
    public string? Format { get; set; }
}
