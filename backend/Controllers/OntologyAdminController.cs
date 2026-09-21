using backend.Contracts.Ontologies;
using backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/v1/admin/ontologies")]
[Authorize(Policy = "RdfWrite")]
public class OntologyAdminController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OntologySummaryDto>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await dbContext.Ontologies
            .Include(item => item.AuthorPortfolios)
            .Include(item => item.BaseDocuments)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return Ok(items.Select(ToDto).ToArray());
    }

    [HttpPost]
    public async Task<ActionResult<OntologySummaryDto>> Create(OntologyAdminRequest request, CancellationToken cancellationToken)
    {
        Validate(request);
        var iri = string.IsNullOrWhiteSpace(request.Iri)
            ? $"https://example.org/ontology/{Guid.NewGuid():N}"
            : request.Iri.Trim();
        if (await dbContext.Ontologies.AnyAsync(item => item.Iri == iri, cancellationToken))
            return Conflict("Já existe uma ontologia com este IRI.");

        var ontology = FromRequest(request, iri);
        dbContext.Ontologies.Add(ontology);
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = ontology.Id }, ToDto(ontology));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OntologySummaryDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var ontology = await dbContext.Ontologies
            .Include(item => item.AuthorPortfolios)
            .Include(item => item.BaseDocuments)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        return ontology is null ? NotFound() : Ok(ToDto(ontology));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<OntologySummaryDto>> Update(Guid id, OntologyAdminRequest request, CancellationToken cancellationToken)
    {
        Validate(request);
        var ontology = await Load(id, cancellationToken);
        if (ontology is null) return NotFound();
        var iri = string.IsNullOrWhiteSpace(request.Iri) ? ontology.Iri : request.Iri.Trim();
        if (await dbContext.Ontologies.AnyAsync(item => item.Id != id && item.Iri == iri, cancellationToken))
            return Conflict("Já existe uma ontologia com este IRI.");

        ontology.Name = request.Name.Trim();
        ontology.Iri = iri;
        ontology.Description = request.Description.Trim();
        ontology.Documentation = request.Documentation.Trim();
        ontology.Terms = string.Join('\n', request.Terms.Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim()).Distinct());
        ontology.ProfileArea = request.ProfileArea.Trim();
        ontology.ProfileResume = request.ProfileResume.Trim();
        ontology.ProfileSource = request.ProfileSource.Trim();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Replace dependent rows by foreign key instead of mixing RemoveRange with
        // navigation replacement in the same change-tracking graph.
        await dbContext.OntologyAuthorPortfolios
            .Where(author => author.OntologyId == id)
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext.OntologyBaseDocuments
            .Where(document => document.OntologyId == id)
            .ExecuteDeleteAsync(cancellationToken);

        foreach (var entry in dbContext.ChangeTracker.Entries()
                     .Where(entry => entry.Entity is OntologyAuthorPortfolio or OntologyBaseDocument)
                     .ToArray())
        {
            entry.State = EntityState.Detached;
        }

        ontology.AuthorPortfolios.Clear();
        ontology.BaseDocuments.Clear();
        AddChildren(ontology, request);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        var updated = await Load(id, cancellationToken);
        return Ok(ToDto(updated!));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var ontology = await dbContext.Ontologies.FindAsync([id], cancellationToken);
        if (ontology is null) return NotFound();
        dbContext.Ontologies.Remove(ontology);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private async Task<Ontology?> Load(Guid id, CancellationToken cancellationToken) => await dbContext.Ontologies
        .Include(item => item.AuthorPortfolios).Include(item => item.BaseDocuments)
        .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

    private Ontology FromRequest(OntologyAdminRequest request, string iri)
    {
        var ontology = new Ontology { Id = Guid.NewGuid(), Name = request.Name.Trim(), Iri = iri, Description = request.Description.Trim(), Documentation = request.Documentation.Trim(), SourceDocument = request.SourceDocument?.Trim() ?? string.Empty, ProfileArea = request.ProfileArea.Trim(), ProfileResume = request.ProfileResume.Trim(), ProfileSource = request.ProfileSource.Trim(), Terms = string.Join('\n', request.Terms.Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim()).Distinct()) };
        AddChildren(ontology, request);
        return ontology;
    }

    private void AddChildren(Ontology ontology, OntologyAdminRequest request)
    {
        var authors = request.Authors
            .Where(item => !string.IsNullOrWhiteSpace(item.AuthorName))
            .Select(item => new OntologyAuthorPortfolio
            {
                Id = Guid.NewGuid(),
                OntologyId = ontology.Id,
                AuthorName = item.AuthorName.Trim(),
                PortfolioUrl = item.PortfolioUrl.Trim()
            });
        var documents = request.BaseDocuments
            .Where(item => !string.IsNullOrWhiteSpace(item.Link))
            .Select(item => new OntologyBaseDocument
            {
                Id = Guid.NewGuid(),
                OntologyId = ontology.Id,
                Link = item.Link.Trim(),
                Description = item.Description.Trim()
            });
        dbContext.OntologyAuthorPortfolios.AddRange(authors);
        dbContext.OntologyBaseDocuments.AddRange(documents);
    }

    private static void Validate(OntologyAdminRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("O título é obrigatório.");
        if (!string.IsNullOrWhiteSpace(request.Iri) && !Uri.TryCreate(request.Iri.Trim(), UriKind.Absolute, out _))
            throw new ArgumentException("O IRI deve ser uma URL absoluta.");
        if (string.IsNullOrWhiteSpace(request.Description)
            || string.IsNullOrWhiteSpace(request.Documentation)
            || string.IsNullOrWhiteSpace(request.ProfileArea)
            || string.IsNullOrWhiteSpace(request.ProfileResume)
            || string.IsNullOrWhiteSpace(request.ProfileSource)
            || request.Terms.Count == 0
            || request.Terms.Any(string.IsNullOrWhiteSpace)
            || request.Authors.Count == 0
            || request.Authors.Any(author => string.IsNullOrWhiteSpace(author.AuthorName) || string.IsNullOrWhiteSpace(author.PortfolioUrl) || !Uri.TryCreate(author.PortfolioUrl, UriKind.Absolute, out _))
            || request.BaseDocuments.Count == 0
            || request.BaseDocuments.Any(document => string.IsNullOrWhiteSpace(document.Link) || string.IsNullOrWhiteSpace(document.Description) || !Uri.TryCreate(document.Link, UriKind.Absolute, out _)))
        {
            throw new ArgumentException("Todos os campos da documentação, autores e documentos-base são obrigatórios e devem ser válidos.");
        }
    }

    private static OntologySummaryDto ToDto(Ontology item) => new()
    {
        Id = item.Id, Iri = item.Iri, Name = item.Name, Description = item.Description, Namespace = item.Iri,
        Documentation = item.Documentation,
        SourceDocument = item.SourceDocument,
        ProfileArea = item.ProfileArea,
        ProfileResume = item.ProfileResume,
        ProfileSource = item.ProfileSource,
        Terms = item.Terms.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
        Authors = item.AuthorPortfolios.Select(author => new OntologyAuthorPortfolioDto { AuthorName = author.AuthorName, PortfolioUrl = author.PortfolioUrl }).ToArray(),
        BaseDocuments = item.BaseDocuments.Select(document => new OntologyBaseDocumentDto { Link = document.Link, Description = document.Description }).ToArray()
    };
}
