using backend.Contracts.Statistics;
using backend.Infrastructure.Qlever;
using System.Text.Json;

namespace backend.Services.Statistics;

public class StatisticsService : IStatisticsService
{
    private readonly IQleverClient _qleverClient;
    private readonly IOperationalMetricsService _metricsService;

    public StatisticsService(IQleverClient qleverClient, IOperationalMetricsService? metricsService = null)
    {
        _qleverClient = qleverClient;
        _metricsService = metricsService ?? new OperationalMetricsService();
    }

    public async Task<StatisticsOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        int triples = 0;
        int ontologies = 0;

        try
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
            var response = await _qleverClient.ExecuteQueryAsync(query, cancellationToken: cancellationToken);
            using var document = JsonDocument.Parse(response);
            var binding = document.RootElement.GetProperty("results").GetProperty("bindings").EnumerateArray().SingleOrDefault();

            triples = GetInt(binding, "triples");
            ontologies = GetInt(binding, "ontologies");
        }
        catch
        {
            // If QLever is temporarily unavailable, fall back to 0 for RDF counts
            triples = 0;
            ontologies = 0;
        }

        return new StatisticsOverviewDto
        {
            RdfDocuments = triples,
            Ontologies = ontologies,
            SparqlQueries = _metricsService.TotalSparqlQueries,
            FreeSearches = _metricsService.TotalFreeSearches,
            AverageResponseTimeMs = _metricsService.AverageResponseTimeMs,
            SuccessRate = _metricsService.SuccessRate,
            UsageIndex = _metricsService.UsageIndex
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
