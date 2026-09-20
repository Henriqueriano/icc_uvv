using backend.Contracts.Ontologies;

namespace backend.Services.Ontologies;

public interface IOntologyService
{
    Task<IReadOnlyList<OntologySummaryDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<OntologySummaryDto?> GetByIriAsync(string iri, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetClassesAsync(string iri, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetPropertiesAsync(string iri, CancellationToken cancellationToken = default);
}
