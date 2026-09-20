using backend.Contracts.Statistics;

namespace backend.Services.Statistics;

public class StatisticsService : IStatisticsService
{
    public Task<StatisticsOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        var overview = new StatisticsOverviewDto
        {
            RdfDocuments = 1284,
            Ontologies = 42,
            SparqlQueries = 8900,
            FreeSearches = 24300,
            AverageResponseTimeMs = 182,
            SuccessRate = 99.4
        };

        return Task.FromResult(overview);
    }
}
