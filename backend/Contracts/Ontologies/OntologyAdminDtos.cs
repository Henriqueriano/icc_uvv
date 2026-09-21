namespace backend.Contracts.Ontologies;

public class OntologyAdminRequest
{
    public string Name { get; set; } = string.Empty;
    public string Iri { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Documentation { get; set; } = string.Empty;
    public string SourceDocument { get; set; } = string.Empty;
    public string ProfileArea { get; set; } = string.Empty;
    public string ProfileResume { get; set; } = string.Empty;
    public string ProfileSource { get; set; } = string.Empty;
    public IReadOnlyList<string> Terms { get; set; } = [];
    public IReadOnlyList<OntologyAuthorPortfolioDto> Authors { get; set; } = [];
    public IReadOnlyList<OntologyBaseDocumentDto> BaseDocuments { get; set; } = [];
}
