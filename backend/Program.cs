using backend.Infrastructure.Fuseki;
using backend.Infrastructure.Health;
using backend.Infrastructure.Qlever;
using backend.Middleware;
using backend.Options;
using backend.Services.Graphs;
using backend.Services.Rdf;
using backend.Services.RdfValidation;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddOptions<RdfOptions>()
    .BindConfiguration(RdfOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<RdfOptions>>().Value);

builder.Services.AddHttpClient<IFusekiClient, FusekiClient>((sp, client) =>
{
    var options = sp.GetRequiredService<RdfOptions>();
    client.BaseAddress = new Uri(options.FusekiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.QueryTimeoutSeconds);
});

builder.Services.AddHttpClient<IQleverClient, QleverClient>((sp, client) =>
{
    var options = sp.GetRequiredService<RdfOptions>();
    client.BaseAddress = new Uri(options.QleverBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.QueryTimeoutSeconds);
});

builder.Services.AddScoped<IRdfService, RdfService>();
builder.Services.AddScoped<IRdfHealthCheck, RdfHealthCheck>();
builder.Services.AddScoped<IRdfValidationService, RdfValidationService>();
builder.Services.AddScoped<IGraphService, GraphService>();

builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

