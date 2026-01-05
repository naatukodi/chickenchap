using ChickenChap.Api.Domain;
using ChickenChap.Api.Dtos;
using ChickenChap.Api.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ---------- JSON Configuration ----------
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy =
        System.Text.Json.JsonNamingPolicy.CamelCase;
});

// ---------- Cosmos DB Configuration ----------
builder.Services.Configure<CosmosOptions>(
    builder.Configuration.GetSection("Cosmos"));

builder.Services.AddSingleton<ICosmosClientFactory, CosmosClientFactory>();

// Reuse single Cosmos container
builder.Services.AddSingleton(sp =>
{
    var factory = sp.GetRequiredService<ICosmosClientFactory>();
    return factory.Container;
});

// Generic Cosmos repositories
builder.Services.AddScoped(typeof(ICosmosRepository<>), typeof(CosmosRepository<>));

// ---------- Blob Storage Configuration (STEP 7) ----------
builder.Services.Configure<BlobStorageOptions>(
    builder.Configuration.GetSection("BlobStorage"));

builder.Services.AddSingleton<IStorageService, BlobStorageService>();

// ---------- Controllers ----------
builder.Services.AddControllers();

// ---------- Swagger / OpenAPI ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ChickenChap API",
        Version = "v1"
    });

    // Optional XML comments
    // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    // c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// ---------- Ensure Cosmos DB Exists ----------
await app.Services
    .GetRequiredService<ICosmosClientFactory>()
    .EnsureProvisionedAsync();

// ---------- Middleware (Azure Friendly) ----------
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders =
        ForwardedHeaders.XForwardedProto |
        ForwardedHeaders.XForwardedHost
});

// ---------- Path Base Support (Optional) ----------
var pathBase = builder.Configuration["ASPNETCORE_PATHBASE"]; // e.g. "/api"
if (!string.IsNullOrWhiteSpace(pathBase))
{
    app.UsePathBase(pathBase);
}

// ---------- Swagger ----------
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ChickenChap API v1");
    c.RoutePrefix = string.Empty; // Swagger at root
});

// ---------- Authorization (future-ready) ----------
app.UseAuthorization();

// ---------- Routing ----------
app.MapControllers();

app.Run();
