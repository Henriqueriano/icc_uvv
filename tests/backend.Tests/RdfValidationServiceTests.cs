using Xunit;
using backend.Contracts.Sparql;
using backend.Infrastructure.Exceptions;
using backend.Infrastructure.Qlever;
using backend.Infrastructure.Security;
using backend.Middleware;
using backend.Options;
using backend.Services.RdfValidation;
using backend.Services.Sparql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace backend.Tests;

public class RdfValidationServiceTests
{
    [Fact]
    public async Task ValidateAsync_ValidTurtle_ReturnsSuccess()
    {
        var service = new RdfValidationService();

        var result = await service.ValidateAsync(
            "@prefix ex: <http://example.org/> . ex:s ex:p ex:o .",
            "text/turtle");

        Assert.True(result.IsValid);
        Assert.Equal("text/turtle", result.Format);
        Assert.True(result.TripleCount > 0);
    }

    [Fact]
    public async Task ValidateAsync_InvalidTurtle_ThrowsRdfException()
    {
        var service = new RdfValidationService();

        await Assert.ThrowsAsync<RdfException>(() => service.ValidateAsync("this is definitely not rdf", "text/turtle"));
    }

    [Fact]
    public async Task ValidateAsync_FileExceedsLimit_ThrowsArgumentException()
    {
        var service = new RdfValidationService();
        var stream = new MemoryStream(new byte[51 * 1024 * 1024]);
        var file = new FormFile(stream, 0, stream.Length, "file", "sample.ttl");

        await Assert.ThrowsAsync<ArgumentException>(() => service.ValidateAsync(file, "text/turtle"));
    }
}

public class SparqlServiceTests
{
    [Fact]
    public async Task ExecuteQueryAsync_ValidSelectQuery_DelegatesToQleverClient()
    {
        var client = new FakeQleverClient("{\"results\": [] }");
        var service = new SparqlService(client);
        var request = new SparqlQueryRequest
        {
            Query = "SELECT * WHERE { ?s ?p ?o } LIMIT 10",
            DefaultGraph = "icc_uvv"
        };

        var result = await service.ExecuteQueryAsync(request);

        Assert.Equal("{\"results\": [] }", result);
    }

    [Fact]
    public async Task ExecuteQueryAsync_WriteQuery_ThrowsSparqlException()
    {
        var client = new FakeQleverClient("{}");
        var service = new SparqlService(client);
        var request = new SparqlQueryRequest
        {
            Query = "INSERT DATA { <http://example.org/s> <http://example.org/p> <http://example.org/o> . }"
        };

        await Assert.ThrowsAsync<SparqlException>(() => service.ExecuteQueryAsync(request));
    }

    private sealed class FakeQleverClient(string response) : IQleverClient
    {
        public Task<string> ExecuteQueryAsync(string query, string? defaultGraph = null, CancellationToken cancellationToken = default)
        {
            Assert.Contains("SELECT", query, StringComparison.OrdinalIgnoreCase);
            return Task.FromResult(response);
        }
    }
}

public class SecurityUrlValidatorTests
{
    [Fact]
    public void IsAllowedUrl_AllowsPublicHttpsUrl()
    {
        var result = SecurityUrlValidator.IsAllowedUrl("https://example.org/data");

        Assert.True(result);
    }

    [Fact]
    public void IsAllowedUrl_RejectsLoopbackHostByDefault()
    {
        var result = SecurityUrlValidator.IsAllowedUrl("http://localhost:7011/query");

        Assert.False(result);
    }

    [Fact]
    public void ValidateConfiguredUrl_ThrowsForPrivateHost()
    {
        var options = new SecurityOptions
        {
            AllowedHosts = ["example.org"]
        };

        Assert.Throws<InvalidOperationException>(() =>
            SecurityUrlValidator.ValidateConfiguredUrl("http://127.0.0.1:8080", "QleverBaseUrl", options));
    }
}

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenDependencyUnavailable_RespondsWithProblemDetails()
    {
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new DependencyUnavailableException("QLever unavailable"),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        var context = new DefaultHttpContext();
        context.TraceIdentifier = "trace-123";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
        Assert.Contains("application/problem+json", context.Response.ContentType);

        context.Response.Body.Position = 0;
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Contains("QLever unavailable", responseBody);
        Assert.Contains("trace-123", responseBody);
    }
}
