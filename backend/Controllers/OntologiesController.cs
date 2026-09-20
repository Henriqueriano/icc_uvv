using backend.Services.Ontologies;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class OntologiesController : ControllerBase
{
    private readonly IOntologyService _ontologyService;

    public OntologiesController(IOntologyService ontologyService)
    {
        _ontologyService = ontologyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var ontologies = await _ontologyService.ListAsync(cancellationToken);
        return Ok(ontologies);
    }

    [HttpGet("{iri}")]
    public async Task<IActionResult> GetByIri(string iri, CancellationToken cancellationToken)
    {
        var ontology = await _ontologyService.GetByIriAsync(iri, cancellationToken);
        if (ontology is null)
        {
            return NotFound();
        }

        return Ok(ontology);
    }

    [HttpGet("{iri}/classes")]
    public async Task<IActionResult> GetClasses(string iri, CancellationToken cancellationToken)
    {
        var classes = await _ontologyService.GetClassesAsync(iri, cancellationToken);
        return Ok(classes);
    }

    [HttpGet("{iri}/properties")]
    public async Task<IActionResult> GetProperties(string iri, CancellationToken cancellationToken)
    {
        var properties = await _ontologyService.GetPropertiesAsync(iri, cancellationToken);
        return Ok(properties);
    }
}
