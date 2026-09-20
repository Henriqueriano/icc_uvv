namespace backend.Contracts.Rdf;

public class GraphStatisticsDto
{
    public string Name { get; set; } = string.Empty;
    public long TripleCount { get; set; }
    public int QueryCount { get; set; }
    public DateTimeOffset LastUpdatedUtc { get; set; } = DateTimeOffset.UtcNow;
}
