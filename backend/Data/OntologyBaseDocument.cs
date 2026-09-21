namespace backend.Data;

public class OntologyBaseDocument
{
    public Guid Id { get; set; }
    public Guid OntologyId { get; set; }
    public required string Link { get; set; }
    public required string Description { get; set; }
    public Ontology Ontology { get; set; } = null!;
}
