using backend.Contracts.Ontologies;
using backend.Infrastructure.Qlever;

namespace backend.Services.Ontologies;

public class OntologyService : IOntologyService
{
    private readonly IQleverClient _qleverClient;

    public OntologyService(IQleverClient qleverClient)
    {
        _qleverClient = qleverClient;
    }

    public Task<IReadOnlyList<OntologySummaryDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var ontologies = new List<OntologySummaryDto>
        {
            new()
            {
                Iri = "http://xmlns.com/foaf/0.1/",
                Name = "FOAF",
                Description = "Ontology for describing people and social relationships.",
                Namespace = "http://xmlns.com/foaf/0.1/",
                ClassCount = 0,
                PropertyCount = 0
            },
            new()
            {
                Iri = "http://www.w3.org/ns/dcat#",
                Name = "DCAT",
                Description = "Ontology for datasets and catalog metadata.",
                Namespace = "http://www.w3.org/ns/dcat#",
                ClassCount = 0,
                PropertyCount = 0
            },
            new()
            {
                Iri = "https://schema.org/",
                Name = "Schema.org",
                Description = "General ontology for describing entities, places and organizations.",
                Namespace = "https://schema.org/",
                ClassCount = 0,
                PropertyCount = 0
            }
        };

        return Task.FromResult<IReadOnlyList<OntologySummaryDto>>(ontologies);
    }

    public Task<OntologySummaryDto?> GetByIriAsync(string iri, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(iri))
        {
            throw new ArgumentException("The ontology IRI is required.", nameof(iri));
        }

        var ontology = new OntologySummaryDto
        {
            Iri = iri,
            Name = "Ontology",
            Description = "Ontology metadata generated from the configured RDF dataset.",
            Namespace = iri,
            ClassCount = 0,
            PropertyCount = 0
        };

        return Task.FromResult<OntologySummaryDto?>(ontology);
    }

    public async Task<IReadOnlyList<string>> GetClassesAsync(string iri, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(iri))
        {
            throw new ArgumentException("The ontology IRI is required.", nameof(iri));
        }

        const string query = @"
            SELECT DISTINCT ?class
            WHERE {
              ?class a owl:Class .
            }
            LIMIT 20
        ";

        var response = await _qleverClient.ExecuteQueryAsync(query, null, cancellationToken);
        return new[] { response };
    }

    public async Task<IReadOnlyList<string>> GetPropertiesAsync(string iri, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(iri))
        {
            throw new ArgumentException("The ontology IRI is required.", nameof(iri));
        }

        const string query = @"
            SELECT DISTINCT ?property
            WHERE {
              ?property a rdf:Property .
            }
            LIMIT 20
        ";

        var response = await _qleverClient.ExecuteQueryAsync(query, null, cancellationToken);
        return new[] { response };
    }
}
