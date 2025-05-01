using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TalkLikeTv.Services;
using TalkLikeTv.Services.Abstractions;

namespace TalkLikeTv.IntegrationTests.Services
{
    public class AzureTranslateIdServiceTests : IDisposable
    {
        private readonly IAzureTranslateService _translateService;
        private readonly ServiceProvider _serviceProvider;

        public AzureTranslateIdServiceTests()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            if (!string.Equals(environment, "GitHub", StringComparison.OrdinalIgnoreCase))
            {
                var configuration = new ConfigurationBuilder()
                    .AddUserSecrets<AzureTranslateIdServiceTests>()
                    .Build();
                
                // Set required environment variables from user secrets before creating the service
                Environment.SetEnvironmentVariable("AZURE_TENANT_ID", configuration["AZURE_TENANT_ID"]);
                Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", configuration["AZURE_CLIENT_ID"]);
                Environment.SetEnvironmentVariable("AZURE_CLIENT_SECRET", configuration["AZURE_CLIENT_SECRET"]);
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT",
                    configuration["ASPNETCORE_ENVIRONMENT"]);
                Environment.SetEnvironmentVariable("AZURE_TRANSLATE_ENDPOINT",
                    configuration["AZURE_TRANSLATE_ENDPOINT"]);
            }

            // Set up dependency injection
            var services = new ServiceCollection();
            services.AddSingleton<IAzureTranslateService, AzureTranslateIdService>();
            _serviceProvider = services.BuildServiceProvider();
            _translateService = _serviceProvider.GetRequiredService<IAzureTranslateService>();
        }

        [Fact]
        public async Task DetectLanguageFromPhrasesAsync_WithEnglishPhrases_ShouldReturnEn()
        {
            // Arrange
            var phrases = new List<string>
            {
                "Hello, how are you?",
                "Today is a beautiful day.",
                "I am learning to use Azure services."
            };

            // Act
            var result = await _translateService.DetectLanguageFromPhrasesAsync(phrases);

            // Assert
            Assert.Equal("en", result);
        }

        [Fact]
        public async Task DetectLanguageFromPhrasesAsync_WithSpanishPhrases_ShouldReturnEs()
        {
            // Arrange
            var phrases = new List<string>
            {
                "Hola, ¿cómo estás?",
                "Hoy es un día hermoso.",
                "Estoy aprendiendo a usar servicios de Azure."
            };

            // Act
            var result = await _translateService.DetectLanguageFromPhrasesAsync(phrases);

            // Assert
            Assert.Equal("es", result);
        }

        [Fact]
        public async Task DetectLanguageFromPhrasesAsync_WithLessThanThreePhrases_ShouldThrowArgumentException()
        {
            // Arrange
            var phrases = new List<string>
            {
                "Hello",
                "World"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _translateService.DetectLanguageFromPhrasesAsync(phrases));
        }

        [Fact]
        public async Task TranslatePhrasesAsync_FromEnglishToSpanish_ShouldReturnSpanishTranslations()
        {
            // Arrange
            var phrases = new List<string>
            {
                "Hello, world!",
                "The sky is blue."
            };

            // Act
            var translations = await _translateService.TranslatePhrasesAsync(phrases, "en", "es");

            // Assert
            Assert.NotNull(translations);
            Assert.Equal(2, translations.Count);
            Assert.Contains("Hola", translations[0]);
            Assert.Contains("cielo", translations[1]);
        }

        [Fact]
        public async Task TranslatePhrasesAsync_FromEnglishToFrench_ShouldReturnFrenchTranslations()
        {
            // Arrange
            var phrases = new List<string>
            {
                "Good morning",
                "How are you?"
            };

            // Act
            var translations = await _translateService.TranslatePhrasesAsync(phrases, "en", "fr");

            // Assert
            Assert.NotNull(translations);
            Assert.Equal(2, translations.Count);
            Assert.Contains("Bonjour", translations[0], StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Comment", translations[1], StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task DetectLanguageFromPhrasesAsync_WithDifferentLanguages_ShouldThrowException()
        {
            // Arrange
            var phrases = new List<string>
            {
                "Hello, how are you?",  // English
                "Bonjour, comment ça va?",  // French
                "Hola, ¿cómo estás?"   // Spanish
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _translateService.DetectLanguageFromPhrasesAsync(phrases));
        }

        [Fact]
        public async Task TranslatePhrasesAsync_WithCancellationToken_ShouldRespectCancellation()
        {
            // Arrange
            var phrases = new List<string>
            {
                "This is a very long text that will take time to translate.",
                "Another very long text that will take significant time to process."
            };

            using var cts = new CancellationTokenSource();
            cts.Cancel(); // Cancel immediately

            // Act & Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => 
                _translateService.TranslatePhrasesAsync(phrases, "en", "fr", cts.Token));
        }

        public void Dispose()
        {
            _serviceProvider.Dispose();
        }
    }
}