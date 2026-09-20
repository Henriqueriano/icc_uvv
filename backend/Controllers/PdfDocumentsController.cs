using backend.Services.Pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace backend.Controllers;

[ApiController]
[Route("api/v1/pdf")]
[EnableRateLimiting("default")]
public class PdfDocumentsController : ControllerBase
{
    private readonly IPdfImportService _pdfImportService;

    public PdfDocumentsController(IPdfImportService pdfImportService)
    {
        _pdfImportService = pdfImportService;
    }

    [Authorize(Policy = "RdfWrite")]
    [HttpPost("import")]
    public async Task<IActionResult> Import(
        IFormFile file,
        [FromForm] string graphName,
        CancellationToken cancellationToken)
    {
        var result = await _pdfImportService.ImportAsync(file, graphName, cancellationToken);
        return Ok(new
        {
            status = "imported",
            graphName = string.IsNullOrWhiteSpace(graphName) ? "default" : graphName,
            result.TripleCount,
            result.Format
        });
    }
}
