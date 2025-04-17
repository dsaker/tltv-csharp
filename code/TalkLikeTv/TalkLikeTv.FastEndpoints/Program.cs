using System.Text.Json;
using System.Text.Json.Serialization;
using FastEndpoints;
using Microsoft.Extensions.Caching.Hybrid;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Register the database context
builder.Services.AddDbContext<TalkliketvContext>();

// Add FastEndpoints with custom serializer options
builder.Services.AddFastEndpoints();

// Register HybridCache
#pragma warning disable EXTEXP0018
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromSeconds(120),
        LocalCacheExpiration = TimeSpan.FromSeconds(60)
    };
});
#pragma warning restore EXTEXP0018

// Register repositories
builder.Services.AddScoped<IVoiceRepository, VoiceRepository>();
builder.Services.AddScoped<ITitleRepository, TitleRepository>();
builder.Services.AddScoped<ILanguageRepository, LanguageRepository>();

var app = builder.Build();

// Configure FastEndpoints with the same JSON options
app.UseFastEndpoints(c =>
{
    c.Serializer.Options.ReferenceHandler = ReferenceHandler.Preserve;
    c.Serializer.Options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    c.Serializer.Options.MaxDepth = 32;
    c.Serializer.Options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    c.Serializer.Options.WriteIndented = true;
    c.Serializer.Options.Converters.Add(new CustomVoiceConverter()); // Add it here as well
});

app.MapGet("/", () => """
                      Hello FastEndpoints!

                      GET /voices/
                      GET /voices/<languageId>
                      
                      GET /languages
                      GET /languages/<languageId>
                      GET /languages/<tag>/{Tag} 
                      
                      GET /titles'
                      GET /titles/<titleId>
                      GET /titles/name/{Name}
                      POST /titles
                      DELETE /titles/<titleId>
                      PUT /titles/<titleId>
                      """);

app.Run();

// Reference for Testing Purposes of FastEndpoints
namespace TalkLikeTv.FastEndpoints
{
    public partial class Program { }
}