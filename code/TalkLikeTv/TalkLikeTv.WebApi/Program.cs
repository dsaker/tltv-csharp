using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Caching.Hybrid; // To use IOutputFormatter.
using TalkLikeTv.EntityModels;
using TalkLikeTv.Repositories;
using TalkLikeTv.Services;
using TalkLikeTv.Services.Abstractions;
using TalkLikeTv.WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTalkliketvContext();
// Add services to the container.

builder.Services.AddControllers(options =>
    {
        WriteLine("Default output formatters:");
        foreach (var formatter in options.OutputFormatters)
        {
            var mediaFormatter = formatter as OutputFormatter;
            if (mediaFormatter is null)
            {
                WriteLine($"  {formatter.GetType().Name}");
            }
            else // OutputFormatter class has SupportedMediaTypes.
            {
                WriteLine("  {0}, Media types: {1}",
                    arg0: mediaFormatter.GetType().Name,
                    arg1: string.Join(", ",
                        mediaFormatter.SupportedMediaTypes));
            }
        }
    })
    .AddXmlDataContractSerializerFormatters()
    .AddXmlSerializerFormatters();

// Add TalkLikeTv features
builder.Services.AddTalkliketvFeatures(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler(appBuilder => {
    appBuilder.Run(async context => {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        var exceptionDetails = context.Features.Get<IExceptionHandlerFeature>();
        if (exceptionDetails != null)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(exceptionDetails.Error, 
                "An unhandled exception occurred during request {Path}", 
                context.Request.Path);
        }
        
        await context.Response.WriteAsync(
            System.Text.Json.JsonSerializer.Serialize(new { 
                error = "An unexpected error occurred" 
            }));
    });
});

app.UseHttpsRedirection();

app.UseResponseCaching();

app.UseAuthorization();

app.MapControllers();

app.Run();
