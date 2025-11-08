using ChickenChap.Api.Domain;
using ChickenChap.Api.Dtos;
using ChickenChap.Api.Infrastructure;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

builder.Services.Configure<CosmosOptions>(builder.Configuration.GetSection("Cosmos"));
builder.Services.AddSingleton<ICosmosClientFactory, CosmosClientFactory>();

// Single container, reused by all repositories
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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ChickenChap API", Version = "v1" });
});

var app = builder.Build();

await app.Services.GetRequiredService<ICosmosClientFactory>().EnsureProvisionedAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
