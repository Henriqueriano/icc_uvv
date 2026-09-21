namespace backend.Data;

public class OntologyAuthorPortfolio
{
    public Guid Id { get; set; }
    public Guid OntologyId { get; set; }
    public required string AuthorName { get; set; }
    public required string PortfolioUrl { get; set; }
    public Ontology Ontology { get; set; } = null!;
}
