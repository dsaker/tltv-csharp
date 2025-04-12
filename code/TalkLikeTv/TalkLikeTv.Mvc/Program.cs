using Microsoft.Data.SqlClient;
using TalkLikeTv.EntityModels;
using AspNetCoreRateLimit;
using DotEnv.Core;
using TalkLikeTv.Mvc.Extensions;


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