using ChickenChap.Api.Domain;
using ChickenChap.Api.Dtos;
using ChickenChap.Api.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services ----------
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

builder.Services.Configure<CosmosOptions>(builder.Configuration.GetSection("Cosmos"));
builder.Services.AddSingleton<ICosmosClientFactory, CosmosClientFactory>();

// Single Cosmos container reused by all repositories
builder.Services.AddSingleton(sp =>
{
    var fac = sp.GetRequiredService<ICosmosClientFactory>();
    return fac.Container;
});

builder.Services.AddScoped<ICosmosRepository<EggCollection>>(sp =>
    new CosmosRepository<EggCollection>(sp.GetRequiredService<Microsoft.Azure.Cosmos.Container>()));
builder.Services.AddScoped<ICosmosRepository<Sale>>(sp =>
    new CosmosRepository<Sale>(sp.GetRequiredService<Microsoft.Azure.Cosmos.Container>()));
builder.Services.AddScoped<ICosmosRepository<FeedUsage>>(sp =>
    new CosmosRepository<FeedUsage>(sp.GetRequiredService<Microsoft.Azure.Cosmos.Container>()));
builder.Services.AddScoped<ICosmosRepository<MedRecord>>(sp =>
    new CosmosRepository<MedRecord>(sp.GetRequiredService<Microsoft.Azure.Cosmos.Container>()));
builder.Services.AddScoped<ICosmosRepository<Expense>>(sp =>
    new CosmosRepository<Expense>(sp.GetRequiredService<Microsoft.Azure.Cosmos.Container>()));
builder.Services.AddScoped<ICosmosRepository<HatchBatch>>(sp =>
    new CosmosRepository<HatchBatch>(sp.GetRequiredService<Microsoft.Azure.Cosmos.Container>()));

builder.Services.AddControllers();

// Swagger/OpenAPI (register once)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ChickenChap API", Version = "v1" });
    // If you generate XML docs, include them here for nicer Swagger:
    // var xml = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xml));
});

var app = builder.Build();

// Ensure Cosmos objects exist (Serverless-safe in your factory)
await app.Services.GetRequiredService<ICosmosClientFactory>().EnsureProvisionedAsync();

// ---------- Middleware (Azure friendly) ----------
// Forward proto/host so Swagger builds correct URLs behind Azure's proxy
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
});

// Support hosting under a virtual path (set in Azure App Settings if needed)
var pathBase = builder.Configuration["ASPNETCORE_PATHBASE"]; // e.g. "/api"
if (!string.IsNullOrWhiteSpace(pathBase))
{
    app.UsePathBase(pathBase);
}

// Enable Swagger UI in any environment
app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ChickenChap API v1");
    c.RoutePrefix = string.Empty;
});


// (optional) HTTPS redirection if you want
// app.UseHttpsRedirection();

app.MapControllers();

app.Run();
