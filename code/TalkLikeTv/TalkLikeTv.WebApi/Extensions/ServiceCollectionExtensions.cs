using Microsoft.AspNetCore.HttpLogging;
using TalkLikeTv.Services.Abstractions;

namespace TalkLikeTv.WebApi.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Services;
using Repositories;
using AspNetCoreRateLimit;
using Microsoft.Extensions.Caching.Hybrid;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTalkliketvFeatures(this IServiceCollection services, IConfiguration configuration)
    {

        services.Configure<TalkliketvOptions>(configuration.GetSection("TalkLikeTv"));

        // Register Services
        services.AddTransient<PatternService>();
        services.AddScoped<ITranslationService, TranslationService>();
        services.AddSingleton<IPhraseService, PhraseService>();
        services.AddScoped<ITokenService, TokenService>(); 
        services.AddSingleton<IAzureTextToSpeechService, AzureTextToSpeechService>();
        services.AddScoped<IAudioFileService, AudioFileService>();
        services.AddScoped<IAzureTranslateService, AzureTranslateService>();
        services.AddScoped<IAudioProcessingService, AudioProcessingService>();
        services.AddSingleton<IZipDirService, ZipDirService>(_ =>
            new ZipDirService(new System.IO.Abstractions.FileSystem()));
        services.AddSingleton<IParseService, ParseService>();
        
        // Register Repositories
        services.AddScoped<ITitleRepository, TitleRepository>();
        services.AddScoped<ILanguageRepository, LanguageRepository>();
        services.AddScoped<IVoiceRepository, VoiceRepository>();
        services.AddScoped<IPhraseRepository, PhraseRepository>();
        services.AddScoped<ITokenRepository, TokenRepository>();
        services.AddScoped<ITranslateRepository, TranslateRepository>();

        // Add rate limiting services
        services.AddMemoryCache();
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        services.AddInMemoryRateLimiting();

        // Add Hybrid Cache
        #pragma warning disable EXTEXP0018
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromSeconds(120),
                LocalCacheExpiration = TimeSpan.FromSeconds(60)
            };
        });
        #pragma warning restore EXTEXP0018
        
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        services.AddOpenApi();
        services.AddResponseCaching();
        
        services.AddHttpLogging(options =>
        {
            // Add the Origin header so it will not be redacted.
            options.RequestHeaders.Add("Origin");

            options.LoggingFields = HttpLoggingFields.All;
            options.RequestBodyLogLimit = 4096; // Default is 32k.
            options.ResponseBodyLogLimit = 4096; // Default is 32k.
        });

        services.AddCors(options =>
        {
            options.AddPolicy(name: "TalkLikeTv.WebApi.Policy",
                policy =>
                {
                    policy.WithOrigins("https://localhost:7215/");
                });
        });

        return services;
    }
}