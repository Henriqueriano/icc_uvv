using backend.Contracts.Rdf;
using backend.Services.RdfValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/v1/rdf")]
public class RdfDocumentsController : ControllerBase
{
    private readonly IRdfValidationService _rdfValidationService;

    public RdfDocumentsController(IRdfValidationService rdfValidationService)
    {
        _rdfValidationService = rdfValidationService;
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
