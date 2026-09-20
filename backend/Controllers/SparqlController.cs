using backend.Contracts.Sparql;
using backend.Services.Sparql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SparqlController : ControllerBase
{
    private readonly ISparqlService _sparqlService;

    public SparqlController(ISparqlService sparqlService)
    {
        _sparqlService = sparqlService;
    }

    [Authorize(Policy = "RdfRead")]
    [HttpPost("query")]
    public async Task<IActionResult> Query([FromBody] SparqlQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await _sparqlService.ExecuteQueryAsync(request, cancellationToken);
        return Content(result, "application/sparql-results+json");
    }

    [Authorize(Policy = "RdfRead")]
    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] SparqlQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await _sparqlService.ValidateAsync(request.Query, cancellationToken);
        return Ok(new { validation = result });
    }
}
