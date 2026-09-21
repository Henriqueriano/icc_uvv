using backend.Contracts.Ontologies;
using backend.Infrastructure.Qlever;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace backend.Services.Ontologies;

public class OntologyService : IOntologyService
{
    private readonly IQleverClient _qleverClient;
    private readonly AppDbContext _dbContext;

    public OntologyService(IQleverClient qleverClient, AppDbContext dbContext)
    {
        _qleverClient = qleverClient;
        _dbContext = dbContext;
    }

    public Task<IReadOnlyList<OntologySummaryDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        return LoadAsync(cancellationToken);
    }

    public async Task<OntologySummaryDto?> GetByIriAsync(string iri, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(iri))
        {
            throw new ArgumentException("The ontology IRI is required.", nameof(iri));
        }

        var ontologies = await LoadAsync(cancellationToken);
        return ontologies.FirstOrDefault(item => item.Iri == iri);
    }

    public async Task<IReadOnlyList<string>> GetClassesAsync(string iri, CancellationToken cancellationToken = default)
    {
        var ontologyIri = NormalizeIri(iri);

        var query = $@"
            PREFIX owl: <http://www.w3.org/2002/07/owl#>
            PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
            SELECT DISTINCT ?class
            WHERE {{
              ?class a owl:Class .
              ?class rdfs:isDefinedBy <{ontologyIri}> .
            }}
            LIMIT 20
            ";

        var response = await _qleverClient.ExecuteQueryAsync(query, cancellationToken: cancellationToken);
        return ParseValues(response, "class");
    }

    public async Task<IReadOnlyList<string>> GetPropertiesAsync(string iri, CancellationToken cancellationToken = default)
    {
        var ontologyIri = NormalizeIri(iri);

        var query = $@"
            PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
            PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
            SELECT DISTINCT ?property
            WHERE {{
              ?property a rdf:Property .
              ?property rdfs:isDefinedBy <{ontologyIri}> .
            }}
            LIMIT 20
            ";

        var response = await _qleverClient.ExecuteQueryAsync(query, cancellationToken: cancellationToken);
        return ParseValues(response, "property");
    }

    private async Task<IReadOnlyList<OntologySummaryDto>> LoadAsync(CancellationToken cancellationToken)
    {
        var documented = await _dbContext.Ontologies
            .Include(ontology => ontology.AuthorPortfolios)
            .Include(ontology => ontology.BaseDocuments)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        var documentedDtos = documented.Select(ontology => new OntologySummaryDto
        {
            Id = ontology.Id,
            Iri = ontology.Iri,
            Name = ontology.Name,
            Description = ontology.Description,
            Documentation = ontology.Documentation,
            SourceDocument = ontology.SourceDocument,
            ProfileArea = ontology.ProfileArea,
            ProfileResume = ontology.ProfileResume,
            ProfileSource = ontology.ProfileSource,
            Namespace = ontology.Iri,
            Terms = ontology.Terms.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            Authors = ontology.AuthorPortfolios.Select(author => new OntologyAuthorPortfolioDto
            {
                AuthorName = author.AuthorName,
                PortfolioUrl = author.PortfolioUrl
            }).ToArray(),
            BaseDocuments = ontology.BaseDocuments.Select(document => new OntologyBaseDocumentDto
            {
                Link = document.Link,
                Description = document.Description
            }).ToArray()
        }).ToList();

        return documentedDtos;
    }

    private static IReadOnlyList<string> ParseValues(string response, string variable)
    {
        using var document = JsonDocument.Parse(response);
        return document.RootElement.GetProperty("results").GetProperty("bindings").EnumerateArray()
            .Select(binding => binding.GetProperty(variable).GetProperty("value").GetString()!)
            .ToArray();
    }

    private static string NormalizeIri(string iri)
    {
        if (string.IsNullOrWhiteSpace(iri)
            || !Uri.TryCreate(iri.Trim(), UriKind.Absolute, out var uri)
            || string.IsNullOrWhiteSpace(uri.Scheme))
        {
            throw new ArgumentException("The ontology IRI must be an absolute URI.", nameof(iri));
        }

        return uri.AbsoluteUri;
    }
}
