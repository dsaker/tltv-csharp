using DotEnv.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TalkLikeTv.WebApi;

namespace TalkLikeTv.IntegrationTests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Load .env.test file from the test project directory
            new EnvLoader().AddEnvFile("test.env").Load();
            
            base.ConfigureWebHost(builder);
        }
}