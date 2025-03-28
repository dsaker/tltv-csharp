using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
// To use SqlConnectionStringBuilder.
// To use UseSqlServer.

// To use IServiceCollection.

namespace TalkLikeTv.EntityModels;

public static class TalkliketvContextExtensions
{
    /// <summary>
    ///     Adds TalkliketvContext to the specified IServiceCollection. Uses the SqlServer database provider.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">Set to override the default.</param>
    /// <returns>An IServiceCollection that can be used to add more services.</returns>
    public static IServiceCollection AddTalkliketvContext(
        this IServiceCollection services, // The type to extend.
        string? connectionString = null)
    {
        if (connectionString is null)
        {
            SqlConnectionStringBuilder builder = new();
            builder.DataSource = "tcp:127.0.0.1,1433"; // SQL Edge in Docker.
            builder.InitialCatalog = "Talkliketv";
            builder.TrustServerCertificate = true;
            builder.MultipleActiveResultSets = true;
            // Because we want to fail faster. Default is 15 seconds.
            builder.ConnectTimeout = 3;
            // SQL Server authentication.
            builder.UserID = Environment.GetEnvironmentVariable("MY_SQL_USR");
            builder.Password = Environment.GetEnvironmentVariable("MY_SQL_PWD");
            connectionString = builder.ConnectionString;
        }

        services.AddDbContext<TalkliketvContext>(options =>
            {
                options.UseSqlServer(connectionString);
                options.LogTo(TalkliketvContextLogger.WriteLine,
                    new[] { RelationalEventId.CommandExecuting });
            },
            // Register with a transient lifetime to avoid concurrency
            // issues with Blazor Server projects.
            ServiceLifetime.Transient,
            ServiceLifetime.Transient);
        return services;
    }
}