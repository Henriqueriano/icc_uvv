using System.Text;
using backend.Contracts.Rdf;
using backend.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Http;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace backend.Services.RdfValidation;

public class RdfValidationService : IRdfValidationService
{
    public Task<RdfValidationResult> ValidateAsync(string content, string format, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("The RDF content cannot be empty.", nameof(content));
        }

        var normalizedFormat = NormalizeFormat(format);

        try
        {
            var parser = CreateParser(normalizedFormat);
            var graph = new Graph();
            graph.LoadFromString(content, parser);

            return Task.FromResult(new RdfValidationResult
            {
                IsValid = true,
                Format = normalizedFormat,
                TripleCount = graph.Triples.Count,
                Message = "RDF content is valid."
            });
        }
        catch (Exception ex) when (ex is VDS.RDF.Parsing.RdfParseException or FormatException or IOException)
        {
            throw new global::backend.Infrastructure.Exceptions.RdfException(
                $"The RDF content is invalid for format '{normalizedFormat}'.",
                ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new TimeoutException("The RDF validation timed out.", ex);
        }
    }

    public async Task<RdfValidationResult> ValidateAsync(IFormFile file, string? format = null, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            throw new ArgumentException("An RDF file is required.", nameof(file));
        }

        if (file.Length > 50 * 1024 * 1024)
        {
            throw new ArgumentException("The RDF file exceeds the maximum allowed size.", nameof(file));
        }

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var content = await reader.ReadToEndAsync(cancellationToken);

        return await ValidateAsync(content, format ?? DetectFormat(file.ContentType, file.FileName), cancellationToken);
    }

    private static string NormalizeFormat(string? format)
    {
        var value = (format ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(value))
        {
            return "text/turtle";
        }

        return value.ToLowerInvariant() switch
        {
            "ttl" or "text/turtle" => "text/turtle",
            "rdfxml" or "application/rdf+xml" => "application/rdf+xml",
            "nt" or "application/n-triples" => "application/n-triples",
            "nq" or "application/n-quads" => "application/n-quads",
            "jsonld" or "application/ld+json" => "application/ld+json",
            _ => value
        };
    }

    private static string DetectFormat(string? contentType, string fileName)
    {
        var normalizedType = (contentType ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(normalizedType))
        {
            return NormalizeFormat(normalizedType);
        }

        var extension = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
        return extension switch
        {
            "ttl" => "text/turtle",
            "rdf" or "xml" => "application/rdf+xml",
            "nt" => "application/n-triples",
            "nq" => "application/n-quads",
            "jsonld" or "json" => "application/ld+json",
            _ => "text/turtle"
        };
    }

    private static IRdfReader CreateParser(string format)
    {
        return format.ToLowerInvariant() switch
        {
            "text/turtle" => new TurtleParser(),
            "application/rdf+xml" => new RdfXmlParser(),
            "application/n-triples" => new NTriplesParser(),
            _ => throw new global::backend.Infrastructure.Exceptions.RdfException(
                $"The RDF format '{format}' is not supported in this validation layer.")
        };
    }
}
