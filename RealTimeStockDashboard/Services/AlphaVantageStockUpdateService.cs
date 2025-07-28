using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RealTimeStockDashboard.Models.AlphaVantage;

namespace RealTimeStockDashboard.Services;

public class AlphaVantageStockUpdateService : BackgroundService
{
    private readonly IHubContext<StockHub> _hubContext;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AlphaVantageStockUpdateService> _logger;
    private readonly Dictionary<string, DateTime> _lastUpdateTimes = new();
    private readonly IConfiguration _configuration;
    private readonly int _updateIntervalMinutes;

    public AlphaVantageStockUpdateService(
        IHubContext<StockHub> hubContext,
        ILogger<AlphaVantageStockUpdateService> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _hubContext = hubContext;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("AlphaVantage");
        _httpClient.Timeout = TimeSpan.FromSeconds(15);
        _configuration = configuration;
        _updateIntervalMinutes = _configuration.GetValue("StockService:UpdateIntervalMinutes", 5);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!StockHubState.HasConnectedClient && !stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(500, stoppingToken);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                foreach (var region in Constants.RegionSymbols)
                {
                    foreach (var symbol in region.Value)
                    {
                        try
                        {
                            if (_lastUpdateTimes.TryGetValue(symbol, out var lastUpdate) &&
                                DateTime.UtcNow - lastUpdate < TimeSpan.FromMinutes(_updateIntervalMinutes))
                            {
                                continue;
                            }

                            await ExecuteRequest(stoppingToken, symbol);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error fetching AlphaVantage quote for {Symbol}", symbol);
                        }

                        await Task.Delay(15000, stoppingToken); // Respect AlphaVantage's 5 requests/minute limit
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // General delay between full cycles
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AlphaVantage update loop");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }

    private async Task ExecuteRequest(CancellationToken stoppingToken, string symbol)
    {
        var url = $"{_configuration["ApiUrls:AlphaVantage"]}{symbol}&apikey={Constants.AlphaVantageKey}";
        var response = await _httpClient.GetFromJsonAsync<AlphaVantageResponse>(url, stoppingToken);

        if (decimal.TryParse(response?.GlobalQuote?.Price, out var price))
        {
            await _hubContext.Clients.All.SendAsync(
                "ReceiveStockUpdate",
                symbol,
                price,
                cancellationToken: stoppingToken);
            _lastUpdateTimes[symbol] = DateTime.UtcNow;
        }
    }
}