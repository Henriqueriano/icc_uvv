namespace backend.Contracts.Rdf;

public class GraphMetadataDto
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "configured";
    public bool IsDefault { get; set; }
    public long TripleCount { get; set; }
    public DateTimeOffset LastUpdatedUtc { get; set; } = DateTimeOffset.UtcNow;
}
