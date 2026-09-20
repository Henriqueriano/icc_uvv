using System.Text;
using System.Threading.RateLimiting;
using backend.Infrastructure.Fuseki;
using backend.Infrastructure.Health;
using backend.Infrastructure.Qlever;
using backend.Infrastructure.Security;
using backend.Data;
using backend.Middleware;
using backend.Options;
using backend.Services.Auditing;
using backend.Services.Authentication;
using backend.Services.Graphs;
using backend.Services.Ontologies;
using backend.Services.Rdf;
using backend.Services.RdfValidation;
using backend.Services.Search;
using backend.Services.Sparql;
using backend.Services.Statistics;
using backend.Scripts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

var environmentFile = FindEnvironmentFile(Directory.GetCurrentDirectory());
if (environmentFile is not null)
{
    DotNetEnv.Env.Load(environmentFile);
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(builder.Configuration["Frontend:Url"] ?? throw new InvalidOperationException("Frontend:Url is required."))
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddOptions<RdfOptions>()
    .BindConfiguration(RdfOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<AuthOptions>()
    .BindConfiguration(AuthOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<SecurityOptions>()
    .BindConfiguration(SecurityOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

var rdfOptions = builder.Configuration.GetSection(RdfOptions.SectionName).Get<RdfOptions>() ?? new RdfOptions();
var authOptions = builder.Configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>() ?? new AuthOptions();
var securityOptions = builder.Configuration.GetSection(SecurityOptions.SectionName).Get<SecurityOptions>() ?? new SecurityOptions();

SecurityUrlValidator.ValidateConfiguredUrl(rdfOptions.FusekiBaseUrl, nameof(RdfOptions.FusekiBaseUrl), securityOptions);
SecurityUrlValidator.ValidateConfiguredUrl(rdfOptions.QleverBaseUrl, nameof(RdfOptions.QleverBaseUrl), securityOptions);

builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<RdfOptions>>().Value);
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<AuthOptions>>().Value);
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<SecurityOptions>>().Value);
builder.Services.AddSingleton<PasswordHashService>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString(authOptions.ConnectionStringName)
        ?? throw new InvalidOperationException("DefaultConnection is required.")));

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
builder.Services.AddScoped<IAuditService, AuditService>();

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

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("default", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = securityOptions.RateLimitPerMinute,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = securityOptions.AuthRateLimitPerMinute,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("search", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = securityOptions.SearchRateLimitPerMinute,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (args.Contains("--seed-admin", StringComparer.Ordinal))
{
    await SeedAdmin.RunAsync(app.Services);
    return;
}

await DatabaseInitializer.InitializeAsync(app.Services);

app.UseCors("Frontend");
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<SecurityValidationMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health/live");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();

app.Run();

static string? FindEnvironmentFile(string startDirectory)
{
    var directory = new DirectoryInfo(startDirectory);
    while (directory is not null)
    {
        var candidate = Path.Combine(directory.FullName, ".env");
        if (File.Exists(candidate))
        {
            return candidate;
        }

        directory = directory.Parent;
    }

    return null;
}
