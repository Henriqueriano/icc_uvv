using backend.Contracts.Statistics;

namespace backend.Services.Statistics;

public interface IStatisticsService
{
    Task<StatisticsOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default);
}
