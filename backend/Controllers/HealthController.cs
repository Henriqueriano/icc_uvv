using System.Diagnostics;
using backend.Contracts.Health;
using backend.Data;
using backend.Infrastructure.Health;
using backend.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace backend.Controllers;

[ApiController]
[Route("[controller]")]
[Route("api/v1/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IRdfHealthCheck _rdfHealthCheck;
    private readonly AppDbContext _dbContext;
    private readonly OllamaOptions _ollamaOptions;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache? _memoryCache;

    public HealthController(
        IRdfHealthCheck rdfHealthCheck,
        AppDbContext dbContext,
        OllamaOptions ollamaOptions,
        IHttpClientFactory httpClientFactory,
        IMemoryCache? memoryCache = null)
    {
        _rdfHealthCheck = rdfHealthCheck;
        _dbContext = dbContext;
        _ollamaOptions = ollamaOptions;
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
    }

    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new { status = "ok" });
    }

    [HttpGet("ready")]
    public async Task<IActionResult> GetReadyAsync(CancellationToken cancellationToken)
    {
        var isReady = await _rdfHealthCheck.IsReadyAsync(cancellationToken);

        if (!isReady)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { status = "not_ready" });
        }

        return Ok(new { status = "ready" });
    }

    [HttpGet("status")]
    [HttpGet("/api/v1/system/status")]
    public async Task<IActionResult> GetStatusAsync(CancellationToken cancellationToken)
    {
        var result = new SystemStatusDto
        {
            Timestamp = DateTimeOffset.UtcNow,
            Server = new ComponentStatusDto
            {
                Status = "Online",
                IsHealthy = true,
                LatencyMs = 0
            }
        };

        // Check RDF base (QLever)
        var qleverSw = Stopwatch.StartNew();
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(3));
            var rdfReady = await _rdfHealthCheck.IsReadyAsync(cts.Token);
            qleverSw.Stop();
            result.RdfBase = new ComponentStatusDto
            {
                Status = rdfReady ? "Saudável" : "Indisponível",
                IsHealthy = rdfReady,
                LatencyMs = qleverSw.ElapsedMilliseconds
            };
        }
        catch
        {
            qleverSw.Stop();
            result.RdfBase = new ComponentStatusDto
            {
                Status = "Indisponível",
                IsHealthy = false,
                LatencyMs = qleverSw.ElapsedMilliseconds
            };
        }

        // Check PostgreSQL database
        var dbSw = Stopwatch.StartNew();
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(3));
            var canConnect = await _dbContext.Database.CanConnectAsync(cts.Token);
            dbSw.Stop();
            result.Database = new ComponentStatusDto
            {
                Status = canConnect ? "Conectado" : "Indisponível",
                IsHealthy = canConnect,
                LatencyMs = dbSw.ElapsedMilliseconds
            };
        }
        catch
        {
            dbSw.Stop();
            result.Database = new ComponentStatusDto
            {
                Status = "Indisponível",
                IsHealthy = false,
                LatencyMs = dbSw.ElapsedMilliseconds
            };
        }

        // Check Ollama AI Service
        var aiSw = Stopwatch.StartNew();
        try
        {
            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(2);
            var uri = new Uri(new Uri(_ollamaOptions.BaseUrl), "/");
            var response = await client.GetAsync(uri, cancellationToken);
            aiSw.Stop();
            result.AiService = new ComponentStatusDto
            {
                Status = response.IsSuccessStatusCode ? "Online" : "Indisponível",
                IsHealthy = response.IsSuccessStatusCode,
                LatencyMs = aiSw.ElapsedMilliseconds,
                Details = _ollamaOptions.Model
            };
        }
        catch
        {
            aiSw.Stop();
            result.AiService = new ComponentStatusDto
            {
                Status = "Indisponível",
                IsHealthy = false,
                LatencyMs = aiSw.ElapsedMilliseconds
            };
        }

        // Check Cache
        result.Cache = new ComponentStatusDto
        {
            Status = _memoryCache != null ? "Ativo" : "Não configurado",
            IsHealthy = _memoryCache != null,
            Details = _memoryCache != null ? "Em memória" : null
        };

        // Evaluate overall integrity
        if (result.RdfBase.IsHealthy && result.Database.IsHealthy)
        {
            result.Integrity = "Normal";
            result.Status = "ready";
        }
        else
        {
            result.Integrity = "Degradada";
            result.Status = result.RdfBase.IsHealthy || result.Database.IsHealthy ? "degraded" : "not_ready";
        }

        return Ok(result);
    }
}
