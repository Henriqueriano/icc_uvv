using System.Text;
using backend.Infrastructure.Fuseki;
using backend.Infrastructure.Health;
using backend.Infrastructure.Qlever;
using backend.Middleware;
using backend.Options;
using backend.Services.Graphs;
using backend.Services.Ontologies;
using backend.Services.Rdf;
using backend.Services.RdfValidation;
using backend.Services.Search;
using backend.Services.Sparql;
using backend.Services.Statistics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddOptions<RdfOptions>()
    .BindConfiguration(RdfOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<AuthOptions>()
    .BindConfiguration(AuthOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<RdfOptions>>().Value);

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<AuthOptions>>().Value);

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
builder.Services.AddScoped<ISparqlService, SparqlService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IOntologyService, OntologyService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();

var authOptions = builder.Configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>() ?? new AuthOptions();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = authOptions.JwtIssuer,
        ValidateAudience = true,
        ValidAudience = authOptions.JwtAudience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.JwtKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RdfWrite", policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole("RdfAdmin"));

    options.AddPolicy("RdfRead", policy =>
        policy.RequireAuthenticatedUser());
});

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

