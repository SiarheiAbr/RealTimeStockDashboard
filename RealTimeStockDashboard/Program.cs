using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RealTimeStockDashboard;
using RealTimeStockDashboard.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddHttpClient("Finnhub");
builder.Services.AddHttpClient("AlphaVantage");

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

switch (builder.Configuration["StockService:Provider"])
{
    case "Finnhub":
        builder.Services.AddHostedService<FinnhubStockUpdateService>();
        break;
    case "AlphaVantage":
        builder.Services.AddHostedService<AlphaVantageStockUpdateService>();
        break;
    default:
        // Fallback to mock service
        builder.Services.AddHostedService<MockStockUpdateService>();
        break;
}

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapHub<StockHub>("/stockHub");

app.Run();
