using backend.Contracts.Search;
using backend.Services.Search;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpPost]
    public async Task<IActionResult> Search([FromBody] SearchRequest request, CancellationToken cancellationToken)
    {
        var result = await _searchService.SearchAsync(request, cancellationToken);
        return Content(result, "application/sparql-results+json");
    }

    [HttpGet("suggestions")]
    public async Task<IActionResult> Suggestions([FromQuery] string text, CancellationToken cancellationToken)
    {
        var result = await _searchService.SuggestionsAsync(text, cancellationToken);
        return Content(result, "application/sparql-results+json");
    }
}
