using backend.Contracts.Rdf;
using backend.Services.Auditing;
using backend.Services.RdfValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace backend.Controllers;

[ApiController]
[Route("api/v1/rdf")]
[EnableRateLimiting("default")]
public class RdfDocumentsController : ControllerBase
{
    private readonly IRdfValidationService _rdfValidationService;
    private readonly IAuditService _auditService;

    public RdfDocumentsController(IRdfValidationService rdfValidationService, IAuditService auditService)
    {
        _rdfValidationService = rdfValidationService;
        _auditService = auditService;
    }

    [HttpPost("import/validate")]
    public async Task<ActionResult<RdfValidationResult>> ValidateImport(
        IFormFile file,
        [FromForm] string? format,
        CancellationToken cancellationToken)
    {
        var result = await _rdfValidationService.ValidateAsync(file, format, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "RdfWrite")]
    [HttpPost("import")]
    public async Task<ActionResult<object>> Import(
        IFormFile file,
        [FromForm] string? format,
        [FromForm] string graphName,
        CancellationToken cancellationToken)
    {
        var result = await _rdfValidationService.ValidateAsync(file, format, cancellationToken);

        await _auditService.AuditAsync(
            "rdf-import",
            string.IsNullOrWhiteSpace(graphName) ? "default" : graphName,
            User.Identity?.Name ?? "anonymous",
            $"format={format}; tripleCount={result.TripleCount}; status=validated",
            true,
            cancellationToken);

        return Ok(new
        {
            id = Guid.NewGuid(),
            status = "validated",
            graphName = string.IsNullOrWhiteSpace(graphName) ? "default" : graphName,
            tripleCount = result.TripleCount,
            format = result.Format
        });
    }
}
