using Xunit;
using backend.Contracts.Health;
using backend.Contracts.Sparql;
using backend.Infrastructure.Health;
using backend.Infrastructure.Qlever;
using backend.Services.Sparql;
using backend.Services.Statistics;

namespace backend.Tests;

public class OperationalMetricsServiceTests
{
    [Fact]
    public void MetricsService_InitialState_ReturnsDefaults()
    {
        var metrics = new OperationalMetricsService();

        Assert.Equal(0, metrics.TotalSparqlQueries);
        Assert.Equal(0, metrics.TotalFreeSearches);
        Assert.Equal(0, metrics.AverageResponseTimeMs);
        Assert.Equal(100.0, metrics.SuccessRate);
        Assert.True(metrics.UsageIndex > 0);
    }

    [Fact]
    public void MetricsService_RecordOperations_CalculatesCorrectly()
    {
        var metrics = new OperationalMetricsService();

        metrics.RecordSparqlQuery(50, true);
        metrics.RecordSparqlQuery(150, true);
        metrics.RecordFreeSearch(100, false);

        Assert.Equal(2, metrics.TotalSparqlQueries);
        Assert.Equal(1, metrics.TotalFreeSearches);
        Assert.Equal(100, metrics.AverageResponseTimeMs); // (50 + 150 + 100) / 3 = 100
        Assert.Equal(66.7, metrics.SuccessRate); // 2 successes out of 3 = 66.7%
    }

    [Fact]
    public void MetricsService_Reset_ClearsAllCounters()
    {
        var metrics = new OperationalMetricsService();
        metrics.RecordSparqlQuery(100, true);
        metrics.Reset();

        Assert.Equal(0, metrics.TotalSparqlQueries);
        Assert.Equal(0, metrics.AverageResponseTimeMs);
        Assert.Equal(100.0, metrics.SuccessRate);
    }
}

public class StatisticsServiceMetricsIntegrationTests
{
    [Fact]
    public async Task GetOverviewAsync_ReturnsRealMetricsFromService()
    {
        var metrics = new OperationalMetricsService();
        metrics.RecordSparqlQuery(40, true);
        metrics.RecordFreeSearch(60, true);

        var fakeQlever = new FakeQleverClient("{\"results\": {\"bindings\": [{\"triples\": {\"value\": \"42\"}, \"ontologies\": {\"value\": \"3\"}}]}}");
        var service = new StatisticsService(fakeQlever, metrics);

        var overview = await service.GetOverviewAsync();

        Assert.Equal(42, overview.RdfDocuments);
        Assert.Equal(3, overview.Ontologies);
        Assert.Equal(1, overview.SparqlQueries);
        Assert.Equal(1, overview.FreeSearches);
        Assert.Equal(50, overview.AverageResponseTimeMs);
        Assert.Equal(100.0, overview.SuccessRate);
        Assert.True(overview.UsageIndex > 0);
    }

    [Fact]
    public async Task GetOverviewAsync_WhenQleverFails_DoesNotThrowAndPreservesMetrics()
    {
        var metrics = new OperationalMetricsService();
        metrics.RecordSparqlQuery(80, true);

        var fakeFailingQlever = new FailingQleverClient();
        var service = new StatisticsService(fakeFailingQlever, metrics);

        var overview = await service.GetOverviewAsync();

        Assert.Equal(0, overview.RdfDocuments);
        Assert.Equal(0, overview.Ontologies);
        Assert.Equal(1, overview.SparqlQueries);
        Assert.Equal(80, overview.AverageResponseTimeMs);
    }

    private sealed class FakeQleverClient(string response) : IQleverClient
    {
        public Task<string> ExecuteQueryAsync(string query, string? defaultGraph = null, CancellationToken cancellationToken = default)
            => Task.FromResult(response);

        public Task<string> PingAsync(CancellationToken cancellationToken = default)
            => Task.FromResult("OK");

        public Task UploadAsync(Stream content, string contentType, string? graphName = null, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FailingQleverClient : IQleverClient
    {
        public Task<string> ExecuteQueryAsync(string query, string? defaultGraph = null, CancellationToken cancellationToken = default)
            => throw new HttpRequestException("Connection refused");

        public Task<string> PingAsync(CancellationToken cancellationToken = default)
            => throw new HttpRequestException("Connection refused");

        public Task UploadAsync(Stream content, string contentType, string? graphName = null, CancellationToken cancellationToken = default)
            => throw new HttpRequestException("Connection refused");
    }
}
