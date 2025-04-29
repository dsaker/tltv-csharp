using DotEnv.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TalkLikeTv.IntegrationTests.Services;
using TalkLikeTv.WebApi;

namespace TalkLikeTv.IntegrationTests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            if (!string.Equals(environment, "GitHub", StringComparison.OrdinalIgnoreCase))
            {
                var configuration = new ConfigurationBuilder()
                    .AddUserSecrets<AzureTranslateEntraIdServiceTests>()
                    .Build();
                
                // Set required environment variables from user secrets before creating the service
                Environment.SetEnvironmentVariable("AZURE_TENANT_ID", configuration["AZURE_TENANT_ID"]);
                Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", configuration["AZURE_CLIENT_ID"]);
                Environment.SetEnvironmentVariable("AZURE_CLIENT_SECRET", configuration["AZURE_CLIENT_SECRET"]);
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", configuration["ASPNETCORE_ENVIRONMENT"]);
                Environment.SetEnvironmentVariable("AZURE_TRANSLATE_ENDPOINT", configuration["AZURE_TRANSLATE_ENDPOINT"]);
            }
            
            base.ConfigureWebHost(builder);
        }
}