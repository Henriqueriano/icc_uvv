using backend.Contracts.Statistics;
using backend.Infrastructure.Fuseki;
using System.Text.Json;

namespace backend.Services.Statistics;

public class StatisticsService : IStatisticsService
{
    private readonly IFusekiClient _fusekiClient;

    public StatisticsService(IFusekiClient fusekiClient)
    {
        _fusekiClient = fusekiClient;
    }

    public async Task<StatisticsOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        const string query = """
            PREFIX owl: <http://www.w3.org/2002/07/owl#>
            SELECT
              (COUNT(*) AS ?triples)
              (COUNT(DISTINCT ?ontology) AS ?ontologies)
            WHERE {
              ?s ?p ?o .
              OPTIONAL { ?ontology a owl:Ontology }
            }
            """;
        var response = await _fusekiClient.ExecuteQueryAsync(query, cancellationToken: cancellationToken);
        using var document = JsonDocument.Parse(response);
        var binding = document.RootElement.GetProperty("results").GetProperty("bindings").EnumerateArray().SingleOrDefault();

        return new StatisticsOverviewDto
        {
            RdfDocuments = GetInt(binding, "triples"),
            Ontologies = GetInt(binding, "ontologies"),
            SparqlQueries = 0,
            FreeSearches = 0,
            AverageResponseTimeMs = 0,
            SuccessRate = 0
        };
    }

    private static int GetInt(JsonElement binding, string property)
    {
        return binding.ValueKind == JsonValueKind.Object
            && binding.TryGetProperty(property, out var value)
            && int.TryParse(value.GetProperty("value").GetString(), out var result)
            ? result
            : 0;
    }
}
