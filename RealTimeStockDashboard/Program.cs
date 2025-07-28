using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RealTimeStockDashboard;
using RealTimeStockDashboard.Extensions;
using RealTimeStockDashboard.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration setup
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// Add services
builder.Services.AddSignalR();
builder.Services.AddHttpClient("Finnhub");
builder.Services.AddHttpClient("AlphaVantage");

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck<StockServiceHealthCheck>(
        "stock_service_health",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "ready" });

// Register the appropriate service based on configuration
var stockServiceProvider = builder.Configuration["StockService:Provider"];
switch (stockServiceProvider)
{
    case "Finnhub":
        builder.Services.AddHostedService<FinnhubStockUpdateService>();
        break;
    case "AlphaVantage":
        builder.Services.AddHostedService<AlphaVantageStockUpdateService>();
        break;
    default:
        builder.Services.AddHostedService<MockStockUpdateService>();
        break;
}

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Add health check endpoint
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = HealthCheckExtensions.WriteResponse
});

app.MapHub<StockHub>("/stockHub");

app.Run();