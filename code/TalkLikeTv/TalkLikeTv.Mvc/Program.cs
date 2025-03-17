using Microsoft.Data.SqlClient;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Mvc.Configurations;
using TalkLikeTv.Services;
using AspNetCoreRateLimit;

var azureKey = Environment.GetEnvironmentVariable("AZURE_TRANSLATE_KEY");
var region = Environment.GetEnvironmentVariable("AZURE_REGION");
if (string.IsNullOrWhiteSpace(azureKey) || string.IsNullOrWhiteSpace(region))
{
    throw new InvalidOperationException("AZURE_TRANSLATE_KEY or REGION environment variable is not set.");
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register TokenService
builder.Services.AddScoped<TokenService>();
builder.Services.Configure<SharedSettings>(builder.Configuration.GetSection("SharedSettings"));

// Add rate limiting services
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddInMemoryRateLimiting();

var sqlServerConnection = builder.Configuration
    .GetConnectionString("TalkliketvConnection");
if (sqlServerConnection is null)
{
    WriteLine("TalkLikeTv database connection string is missing from configuration!");
}
else
{
    // If you are using SQL Server authentication then disable
    // Windows Integrated authentication and set user and password.
    SqlConnectionStringBuilder sql = new(sqlServerConnection);
    sql.IntegratedSecurity = false;
    sql.UserID = Environment.GetEnvironmentVariable("MY_SQL_USR");
    sql.Password = Environment.GetEnvironmentVariable("MY_SQL_PWD");
    builder.Services.AddTalkliketvContext(sql.ConnectionString);
}

var app = builder.Build();

// Call LoadPatterns method
PatternService.LoadPatterns();

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

app.UseAuthorization();

app.MapControllerRoute(
        "default",
        "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();