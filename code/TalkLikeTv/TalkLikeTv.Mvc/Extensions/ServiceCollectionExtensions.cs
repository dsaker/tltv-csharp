using TalkLikeTv.Services.Abstractions;

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

        services.Configure<SharedSettings>(configuration.GetSection("SharedSettings"));

        // Register Services
        services.AddTransient<PatternService>();
        services.AddScoped<ITranslationService, TranslationService>();
        services.AddSingleton<IPhraseService, PhraseService>();
        services.AddScoped<ITokenService, TokenService>(); 
        services.AddSingleton<AzureTextToSpeechService>();
        services.AddScoped<IAudioFileService, AudioFileService>();
        services.AddScoped<ITranslateService, AzureTranslateService>();
        services.AddScoped<IAudioProcessingService, AudioProcessingService>();
        
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