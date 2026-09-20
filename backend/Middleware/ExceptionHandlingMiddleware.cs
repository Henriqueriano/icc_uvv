using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using backend.Infrastructure.Exceptions;

namespace backend.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for request {RequestPath}", context.Request.Path);

            var problemDetails = new ProblemDetails
            {
                Status = MapStatusCode(ex),
                Title = GetTitle(ex),
                Detail = ex.Message,
                Instance = context.Request.Path
            };

            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
            problemDetails.Extensions["traceId"] = traceId;

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

            var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }

    private static int MapStatusCode(Exception ex) => ex switch
    {
        ArgumentException => StatusCodes.Status400BadRequest,
        RdfException => StatusCodes.Status422UnprocessableEntity,
        SparqlException => StatusCodes.Status400BadRequest,
        DependencyUnavailableException => StatusCodes.Status503ServiceUnavailable,
        TimeoutException => StatusCodes.Status504GatewayTimeout,
        _ => StatusCodes.Status500InternalServerError
    };

    private static string GetTitle(Exception ex) => ex switch
    {
        ArgumentException => "Bad request",
        RdfException => "Invalid RDF",
        SparqlException => "Invalid SPARQL",
        DependencyUnavailableException => "Dependency unavailable",
        TimeoutException => "Request timeout",
        _ => "Internal server error"
    };
}
