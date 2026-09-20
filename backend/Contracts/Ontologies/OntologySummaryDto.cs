namespace backend.Contracts.Ontologies;

public class OntologySummaryDto
{
    public string Iri { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public int ClassCount { get; set; }
    public int PropertyCount { get; set; }
}
