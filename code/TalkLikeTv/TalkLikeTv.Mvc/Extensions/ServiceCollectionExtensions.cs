namespace TalkLikeTv.Mvc.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Services;
using Repositories;
using AspNetCoreRateLimit;
using Microsoft.Extensions.Caching.Hybrid;
using Configurations;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTalkliketvFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        // Register TokenService and settings
        services.AddScoped<TokenService>();
        services.Configure<SharedSettings>(configuration.GetSection("SharedSettings"));

        // Register Services
        services.AddTransient<PatternService>();
        services.AddScoped<TranslationService>();
        services.AddSingleton<PhraseService>();
        services.AddSingleton<AzureTextToSpeechService>();
        services.AddSingleton<AzureTranslateService>();
        services.AddScoped<AudioFileService>();
        services.AddScoped<AudioProcessingService>();
        
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

        return services;
    }
}