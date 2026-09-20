using backend.Infrastructure.Health;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly IRdfHealthCheck _rdfHealthCheck;

    public HealthController(IRdfHealthCheck rdfHealthCheck)
    {
        _rdfHealthCheck = rdfHealthCheck;
    }

    [HttpGet("/health")]
    public IActionResult GetHealth()
    {
        return Ok(new { status = "ok" });
    }

    [HttpGet("/health/ready")]
    public async Task<IActionResult> GetReadyAsync(CancellationToken cancellationToken)
    {
        var isReady = await _rdfHealthCheck.IsReadyAsync(cancellationToken);

        if (!isReady)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { status = "not_ready" });
        }

        return Ok(new { status = "ready" });
    }
}
