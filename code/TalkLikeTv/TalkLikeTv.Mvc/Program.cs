using Microsoft.Data.SqlClient;
using TalkLikeTv.EntityModels;
using AspNetCoreRateLimit;
using DotEnv.Core;
using TalkLikeTv.Mvc.Extensions;
using TalkLikeTv.Services;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Load environment variables from .env file
new EnvLoader().Load();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add TalkLikeTv features
builder.Services.AddTalkliketvFeatures(builder.Configuration);

builder.Services.AddInMemoryRateLimiting();

var sqlServerConnection = builder.Configuration
    .GetConnectionString("TalkliketvConnection");
if (sqlServerConnection is not null)
{
    SqlConnectionStringBuilder sql = new(sqlServerConnection);
    sql.IntegratedSecurity = false;
    sql.UserID = Environment.GetEnvironmentVariable("MY_SQL_USR");
    sql.Password = Environment.GetEnvironmentVariable("MY_SQL_PWD");
    
    // Add database connection verification
    try
    {
        using var connection = new SqlConnection(sql.ConnectionString);
        WriteLine("Attempting to connect to the database...");
        connection.Open();
        WriteLine("Successfully connected to the database!");
        connection.Close();
        
        builder.Services.AddTalkliketvContext(sql.ConnectionString);
    }
    catch (SqlException ex)
    {
        WriteLine($"Failed to connect to the database: {ex.Message}");
        WriteLine("Application will now exit due to database connection failure.");
        Environment.Exit(1);
    }
}
else
{
    WriteLine("TalkLikeTv database connection string is missing from configuration!");
    Environment.Exit(1);
}


var app = builder.Build();

// Copy pause files to the destination directory
var pauseFileService = app.Services.GetRequiredService<PauseFileService>();
pauseFileService.EnsurePauseFilesExist();

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

app.UseAuthorization();

app.MapControllerRoute(
        "default",
        "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();