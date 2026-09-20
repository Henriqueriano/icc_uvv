using backend.Contracts.Rdf;
using backend.Services.Auditing;
using backend.Services.RdfValidation;
using backend.Infrastructure.Qlever;
using backend.Options;
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
    private readonly IQleverClient _qleverClient;
    private readonly RdfOptions _rdfOptions;

    public RdfDocumentsController(
        IRdfValidationService rdfValidationService,
        IAuditService auditService,
        IQleverClient qleverClient,
        RdfOptions rdfOptions)
    {
        _rdfValidationService = rdfValidationService;
        _auditService = auditService;
        _qleverClient = qleverClient;
        _rdfOptions = rdfOptions;
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
        await using var stream = file.OpenReadStream();
        var graph = string.IsNullOrWhiteSpace(graphName)
            || string.Equals(graphName.Trim(), "default", StringComparison.OrdinalIgnoreCase)
            || string.Equals(graphName.Trim(), _rdfOptions.QleverIndex, StringComparison.OrdinalIgnoreCase)
            ? null
            : graphName.Trim();
        if (graph is not null
            && (!Uri.TryCreate(graph, UriKind.Absolute, out var graphUri)
                || string.IsNullOrWhiteSpace(graphUri.Scheme)))
        {
            return BadRequest(new { error = "graphName must be an absolute graph URI or 'default'." });
        }

        await _qleverClient.UploadAsync(
            stream,
            result.Format,
            graph,
            cancellationToken);

        await _auditService.AuditAsync(
            "rdf-import",
            string.IsNullOrWhiteSpace(graphName) ? "default" : graphName,
            User.Identity?.Name ?? "anonymous",
            $"format={result.Format}; tripleCount={result.TripleCount}; status=imported",
            true,
            cancellationToken);

        return Ok(new
        {
            id = Guid.NewGuid(),
            status = "imported",
            graphName = graph ?? "default",
            tripleCount = result.TripleCount,
            format = result.Format
        });
    }
}
