using Microsoft.Data.SqlClient;
using TalkLikeTv.EntityModels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

string? sqlServerConnection = builder.Configuration
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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        "default",
        "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();