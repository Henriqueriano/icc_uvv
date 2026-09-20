using backend.Infrastructure.Fuseki;
using backend.Infrastructure.Qlever;
using backend.Middleware;
using backend.Options;
using backend.Services.Rdf;
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

builder.Services.AddHttpClient<IFusekiClient, FusekiClient>();
builder.Services.AddHttpClient<IQleverClient, QleverClient>();
builder.Services.AddScoped<IRdfService, RdfService>();

builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
