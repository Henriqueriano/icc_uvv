namespace backend.Data;

public class Ontology
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Iri { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Documentation { get; set; } = string.Empty;
    public string Terms { get; set; } = string.Empty;
    public string SourceDocument { get; set; } = string.Empty;
    public string ProfileName { get; set; } = string.Empty;
    public string ProfileArea { get; set; } = string.Empty;
    public string ProfileResume { get; set; } = string.Empty;
    public string ProfileSource { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<OntologyAuthorPortfolio> AuthorPortfolios { get; set; } = new List<OntologyAuthorPortfolio>();
    public ICollection<OntologyBaseDocument> BaseDocuments { get; set; } = new List<OntologyBaseDocument>();
}
