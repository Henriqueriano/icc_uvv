using System.Text;
using backend.Contracts.Rdf;
using backend.Infrastructure.Exceptions;
using backend.Infrastructure.Ollama;
using backend.Infrastructure.Qlever;
using backend.Services.RdfValidation;
using UglyToad.PdfPig;

namespace backend.Services.Pdf;

public interface IPdfImportService
{
    Task<PdfImportResult> ImportAsync(IFormFile file, string graphName, CancellationToken cancellationToken = default);
}

public sealed class PdfImportService : IPdfImportService
{
    private readonly IOllamaClient _ollamaClient;
    private readonly IRdfValidationService _rdfValidationService;
    private readonly IQleverClient _qleverClient;

    public PdfImportService(
        IOllamaClient ollamaClient,
        IRdfValidationService rdfValidationService,
        IQleverClient qleverClient)
    {
        _ollamaClient = ollamaClient;
        _rdfValidationService = rdfValidationService;
        _qleverClient = qleverClient;
    }

    public async Task<PdfImportResult> ImportAsync(
        IFormFile file,
        string graphName,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            throw new ArgumentException("A PDF file is required.", nameof(file));
        }

        if (!string.Equals(Path.GetExtension(file.FileName), ".pdf", StringComparison.OrdinalIgnoreCase)
            || !string.Equals(file.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Only PDF files are accepted.", nameof(file));
        }

        if (file.Length > 50 * 1024 * 1024)
        {
            throw new ArgumentException("The PDF file exceeds the maximum allowed size.", nameof(file));
        }

        string extractedText;
        await using (var stream = file.OpenReadStream())
        {
            extractedText = ExtractText(stream);
        }

        if (string.IsNullOrWhiteSpace(extractedText))
        {
            throw new RdfException("The PDF does not contain extractable text.");
        }

        var prompt = $"""
            Transform the PDF text below into valid RDF 1.1 Turtle.
            Return only the Turtle document, without Markdown fences or explanations.
            Use stable HTTP IRIs for subjects and predicates, add prefixes when useful,
            and represent every factual statement as a subject-predicate-object triple.
            Do not invent facts that are not present in the text.

            PDF text:
            {extractedText}
            """;
        var generatedRdf = CleanGeneratedRdf(await _ollamaClient.GenerateAsync(prompt, cancellationToken));
        var validation = await _rdfValidationService.ValidateAsync(generatedRdf, "text/turtle", cancellationToken);
        await using var rdfStream = new MemoryStream(Encoding.UTF8.GetBytes(generatedRdf));
        await _qleverClient.UploadAsync(rdfStream, validation.Format, NormalizeGraph(graphName), cancellationToken);

        return new PdfImportResult(validation.TripleCount, validation.Format);
    }

    private static string ExtractText(Stream stream)
    {
        using var document = PdfDocument.Open(stream);
        var builder = new StringBuilder();
        foreach (var page in document.GetPages())
        {
            builder.AppendLine(page.Text);
        }

        return builder.ToString().Trim();
    }

    private static string CleanGeneratedRdf(string value)
    {
        var rdf = value
            .Replace("```turtle", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("```ttl", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("```", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim();
        if (string.IsNullOrWhiteSpace(rdf))
        {
            throw new RdfException("Ollama returned an empty RDF document.");
        }

        return rdf;
    }

    private static string? NormalizeGraph(string graphName)
    {
        if (string.IsNullOrWhiteSpace(graphName)
            || string.Equals(graphName.Trim(), "default", StringComparison.OrdinalIgnoreCase)
            || string.Equals(graphName.Trim(), "icc_uvv", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (!Uri.TryCreate(graphName.Trim(), UriKind.Absolute, out var graphUri)
            || string.IsNullOrWhiteSpace(graphUri.Scheme))
        {
            throw new ArgumentException("graphName must be an absolute graph URI or 'default'.", nameof(graphName));
        }

        return graphName.Trim();
    }
}

public sealed record PdfImportResult(int TripleCount, string Format);
