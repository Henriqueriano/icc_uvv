using System.Text;
using System.Diagnostics;
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
        string pdfPath;
        var temporaryDirectory = Directory.CreateTempSubdirectory("icc-uvv-pdf-");
        await using (var stream = file.OpenReadStream())
        {
            pdfPath = Path.Combine(temporaryDirectory.FullName, "source.pdf");
            await using var output = File.Create(pdfPath);
            await stream.CopyToAsync(output, cancellationToken);
        }

        try
        {
            await using var stream = File.OpenRead(pdfPath);
            extractedText = ExtractText(stream);
            if (string.IsNullOrWhiteSpace(extractedText))
            {
                extractedText = await ExtractTextWithOcrAsync(pdfPath, temporaryDirectory.FullName, cancellationToken);
            }
        }
        finally
        {
            temporaryDirectory.Delete(recursive: true);
        }

        if (string.IsNullOrWhiteSpace(extractedText))
        {
            throw new RdfException("The PDF does not contain extractable text and OCR could not recover any text.");
        }

        var rdfParts = new List<string>();
        foreach (var textPart in SplitText(extractedText, 12_000))
        {
            var prompt = $"""
                Transform the text below into valid RDF 1.1 Turtle.
                Return only Turtle, without Markdown fences or explanations.
                Use stable HTTP IRIs for subjects and predicates.
                Do not invent facts. This is part of a larger document, so emit
                only triples supported by this part.

                Text:
                {textPart}
                """;
            rdfParts.Add(CleanGeneratedRdf(await _ollamaClient.GenerateAsync(prompt, cancellationToken)));
        }

        var generatedRdf = string.Join(Environment.NewLine, rdfParts);
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

    private static async Task<string> ExtractTextWithOcrAsync(
        string pdfPath,
        string workingDirectory,
        CancellationToken cancellationToken)
    {
        var imagePrefix = Path.Combine(workingDirectory, "page");
        await RunProcessAsync("pdftoppm", $"-png -r 180 \"{pdfPath}\" \"{imagePrefix}\"", cancellationToken);
        var text = new StringBuilder();
        foreach (var imagePath in Directory.EnumerateFiles(workingDirectory, "page-*.png").OrderBy(path => path))
        {
            var outputBase = Path.Combine(workingDirectory, Path.GetFileNameWithoutExtension(imagePath));
            await RunProcessAsync("tesseract", $"\"{imagePath}\" \"{outputBase}\" -l por+eng", cancellationToken);
            var textPath = $"{outputBase}.txt";
            if (File.Exists(textPath))
            {
                text.AppendLine(await File.ReadAllTextAsync(textPath, cancellationToken));
            }
        }

        return text.ToString().Trim();
    }

    private static async Task RunProcessAsync(string command, string arguments, CancellationToken cancellationToken)
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            WorkingDirectory = Directory.GetCurrentDirectory(),
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        }) ?? throw new RdfException($"The PDF processing tool '{command}' could not be started.");

        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);
            throw new RdfException($"The PDF processing tool '{command}' failed: {error.Trim()}");
        }
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

    private static IEnumerable<string> SplitText(string text, int maxLength)
    {
        for (var offset = 0; offset < text.Length; offset += maxLength)
        {
            var length = Math.Min(maxLength, text.Length - offset);
            yield return text.Substring(offset, length);
        }
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
