namespace backend.Contracts.Health;

public class ComponentStatusDto
{
    public string Status { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public long? LatencyMs { get; set; }
    public string? Details { get; set; }
}

public class SystemStatusDto
{
    public string Status { get; set; } = "ready";
    public string Integrity { get; set; } = "Normal";
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public ComponentStatusDto Server { get; set; } = new();
    public ComponentStatusDto RdfBase { get; set; } = new();
    public ComponentStatusDto Database { get; set; } = new();
    public ComponentStatusDto AiService { get; set; } = new();
    public ComponentStatusDto Cache { get; set; } = new();
}
