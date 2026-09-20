using backend.Services.Graphs;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class GraphsController : ControllerBase
{
    private readonly IGraphService _graphService;

    public GraphsController(IGraphService graphService)
    {
        _graphService = graphService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var graphs = await _graphService.ListAsync(cancellationToken);
        return Ok(graphs);
    }

    [HttpGet("{graphName}")]
    public async Task<IActionResult> GetByName(string graphName, CancellationToken cancellationToken)
    {
        var graph = await _graphService.GetByNameAsync(graphName, cancellationToken);
        if (graph is null)
        {
            return NotFound();
        }

        return Ok(graph);
    }

    [HttpGet("{graphName}/statistics")]
    public async Task<IActionResult> GetStatistics(string graphName, CancellationToken cancellationToken)
    {
        var stats = await _graphService.GetStatisticsAsync(graphName, cancellationToken);
        return Ok(stats);
    }

    [HttpGet("{graphName}/content")]
    public async Task<IActionResult> GetContent(string graphName, [FromQuery] string? format, CancellationToken cancellationToken)
    {
        var content = await _graphService.GetContentAsync(graphName, format, cancellationToken);
        return Ok(content);
    }
}
