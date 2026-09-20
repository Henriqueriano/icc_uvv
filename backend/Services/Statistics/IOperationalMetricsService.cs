namespace backend.Services.Statistics;

public interface IOperationalMetricsService
{
    void RecordSparqlQuery(long elapsedMilliseconds, bool success);
    void RecordFreeSearch(long elapsedMilliseconds, bool success);
    int TotalSparqlQueries { get; }
    int TotalFreeSearches { get; }
    int AverageResponseTimeMs { get; }
    double SuccessRate { get; }
    double UsageIndex { get; }
    void Reset();
}
