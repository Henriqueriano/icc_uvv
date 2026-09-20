using backend.Contracts.Rdf;
using Microsoft.AspNetCore.Http;

namespace backend.Services.RdfValidation;

public interface IRdfValidationService
{
    Task<RdfValidationResult> ValidateAsync(string content, string format, CancellationToken cancellationToken = default);
    Task<RdfValidationResult> ValidateAsync(IFormFile file, string? format = null, CancellationToken cancellationToken = default);
}
