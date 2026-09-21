namespace backend.Contracts.Ontologies;

public class OntologySummaryDto
{
    public Guid Id { get; set; }
    public string Iri { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Documentation { get; set; } = string.Empty;
    public string SourceDocument { get; set; } = string.Empty;
    public string ProfileArea { get; set; } = string.Empty;
    public string ProfileResume { get; set; } = string.Empty;
    public string ProfileSource { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public int ClassCount { get; set; }
    public int PropertyCount { get; set; }
    public IReadOnlyList<OntologyAuthorPortfolioDto> Authors { get; set; } = [];
    public IReadOnlyList<OntologyBaseDocumentDto> BaseDocuments { get; set; } = [];
    public IReadOnlyList<string> Terms { get; set; } = [];
}

public class OntologyBaseDocumentDto
{
    public string Link { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class OntologyAuthorPortfolioDto
{
    public string AuthorName { get; set; } = string.Empty;
    public string PortfolioUrl { get; set; } = string.Empty;
}
