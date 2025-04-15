using System.Net.Http.Headers;
using AspNetCoreRateLimit;
using DotEnv.Core;
using Microsoft.Extensions.Caching.Hybrid;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Load environment variables from .env file
new EnvLoader().Load();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add rate limiting services
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddInMemoryRateLimiting();

// Add Hybrid Cache
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
        
builder.Services.AddHttpClient(name: "TalkLikeTv.WebApi",
    configureClient: options =>
    {
        options.BaseAddress = new Uri("https://localhost:7197/");
        options.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue(
                mediaType: "application/json", quality: 1.0));
    });

builder.Services.AddInMemoryRateLimiting();


var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
        "default",
        "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();