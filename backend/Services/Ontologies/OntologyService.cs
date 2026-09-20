using backend.Contracts.Ontologies;
using backend.Infrastructure.Qlever;
using System.Text.Json;

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
        if (string.IsNullOrWhiteSpace(iri))
        {
            throw new ArgumentException("The ontology IRI is required.", nameof(iri));
        }

        const string query = @"
            PREFIX owl: <http://www.w3.org/2002/07/owl#>
            SELECT DISTINCT ?class
            WHERE {
              ?class a owl:Class .
            }
            LIMIT 20
        ";

        var response = await _qleverClient.ExecuteQueryAsync(query, cancellationToken: cancellationToken);
        return ParseValues(response, "class");
    }

    public async Task<IReadOnlyList<string>> GetPropertiesAsync(string iri, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(iri))
        {
            throw new ArgumentException("The ontology IRI is required.", nameof(iri));
        }

        const string query = @"
            PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
            SELECT DISTINCT ?property
            WHERE {
              ?property a rdf:Property .
            }
            LIMIT 20
        ";

        var response = await _qleverClient.ExecuteQueryAsync(query, cancellationToken: cancellationToken);
        return ParseValues(response, "property");
    }

    private async Task<IReadOnlyList<OntologySummaryDto>> LoadAsync(CancellationToken cancellationToken)
    {
        const string query = """
            PREFIX owl: <http://www.w3.org/2002/07/owl#>
            PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
            SELECT ?ontology (COUNT(DISTINCT ?class) AS ?classes) (COUNT(DISTINCT ?property) AS ?properties)
            WHERE {
              { ?ontology a owl:Ontology }
              OPTIONAL { ?class a owl:Class }
              OPTIONAL { ?property a rdf:Property }
            }
            GROUP BY ?ontology
            """;
        var response = await _qleverClient.ExecuteQueryAsync(query, cancellationToken: cancellationToken);
        using var document = JsonDocument.Parse(response);
        return document.RootElement.GetProperty("results").GetProperty("bindings").EnumerateArray()
            .Select(binding => new OntologySummaryDto
            {
                Iri = binding.GetProperty("ontology").GetProperty("value").GetString()!,
                Name = binding.GetProperty("ontology").GetProperty("value").GetString()!,
                Namespace = binding.GetProperty("ontology").GetProperty("value").GetString()!,
                ClassCount = int.Parse(binding.GetProperty("classes").GetProperty("value").GetString()!),
                PropertyCount = int.Parse(binding.GetProperty("properties").GetProperty("value").GetString()!)
            }).ToArray();
    }

    private static IReadOnlyList<string> ParseValues(string response, string variable)
    {
        using var document = JsonDocument.Parse(response);
        return document.RootElement.GetProperty("results").GetProperty("bindings").EnumerateArray()
            .Select(binding => binding.GetProperty(variable).GetProperty("value").GetString()!)
            .ToArray();
    }
}
