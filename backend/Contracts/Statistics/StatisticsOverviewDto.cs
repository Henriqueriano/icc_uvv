namespace backend.Contracts.Statistics;

public class StatisticsOverviewDto
{
    public int RdfDocuments { get; set; }
    public int Ontologies { get; set; }
    public int SparqlQueries { get; set; }
    public int FreeSearches { get; set; }
    public int AverageResponseTimeMs { get; set; }
    public double SuccessRate { get; set; }
    public double UsageIndex { get; set; }
}
