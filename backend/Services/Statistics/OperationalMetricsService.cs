using System.Diagnostics;

namespace backend.Services.Statistics;

public class OperationalMetricsService : IOperationalMetricsService
{
    private long _sparqlQueries;
    private long _freeSearches;
    private long _totalOperations;
    private long _successCount;
    private long _failureCount;
    private long _totalElapsedMs;

    public void RecordSparqlQuery(long elapsedMilliseconds, bool success)
    {
        Interlocked.Increment(ref _sparqlQueries);
        RecordOperation(elapsedMilliseconds, success);
    }

    public void RecordFreeSearch(long elapsedMilliseconds, bool success)
    {
        Interlocked.Increment(ref _freeSearches);
        RecordOperation(elapsedMilliseconds, success);
    }

    private void RecordOperation(long elapsedMilliseconds, bool success)
    {
        Interlocked.Increment(ref _totalOperations);
        Interlocked.Add(ref _totalElapsedMs, Math.Max(0, elapsedMilliseconds));

        if (success)
        {
            Interlocked.Increment(ref _successCount);
        }
        else
        {
            Interlocked.Increment(ref _failureCount);
        }
    }

    public int TotalSparqlQueries => (int)Interlocked.Read(ref _sparqlQueries);

    public int TotalFreeSearches => (int)Interlocked.Read(ref _freeSearches);

    public int AverageResponseTimeMs
    {
        get
        {
            var totalOps = Interlocked.Read(ref _totalOperations);
            if (totalOps == 0)
            {
                return 0;
            }

            var totalTime = Interlocked.Read(ref _totalElapsedMs);
            return (int)(totalTime / totalOps);
        }
    }

    public double SuccessRate
    {
        get
        {
            var totalOps = Interlocked.Read(ref _totalOperations);
            if (totalOps == 0)
            {
                return 100.0;
            }

            var successes = Interlocked.Read(ref _successCount);
            return Math.Round((double)successes / totalOps * 100.0, 1);
        }
    }

    public double UsageIndex
    {
        get
        {
            try
            {
                using var process = Process.GetCurrentProcess();
                var workingSetMb = process.WorkingSet64 / (1024.0 * 1024.0);
                var calculated = Math.Min(95.0, Math.Max(15.0, Math.Round((workingSetMb / 512.0) * 100.0, 1)));
                return calculated;
            }
            catch
            {
                return 25.0;
            }
        }
    }

    public void Reset()
    {
        Interlocked.Exchange(ref _sparqlQueries, 0);
        Interlocked.Exchange(ref _freeSearches, 0);
        Interlocked.Exchange(ref _totalOperations, 0);
        Interlocked.Exchange(ref _successCount, 0);
        Interlocked.Exchange(ref _failureCount, 0);
        Interlocked.Exchange(ref _totalElapsedMs, 0);
    }
}
