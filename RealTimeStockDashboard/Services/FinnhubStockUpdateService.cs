using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RealTimeStockDashboard.Models.Finnhub;

namespace RealTimeStockDashboard.Services;

public class FinnhubStockUpdateService : BackgroundService
{
    private readonly IHubContext<StockHub> _hubContext;
    private readonly HttpClient _httpClient;
    private readonly ILogger<FinnhubStockUpdateService> _logger;
    private readonly IConfiguration _configuration;
    private readonly int _updateIntervalSeconds;

    public FinnhubStockUpdateService(
        IHubContext<StockHub> hubContext,
        ILogger<FinnhubStockUpdateService> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _hubContext = hubContext;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("Finnhub");
        _httpClient.Timeout = TimeSpan.FromSeconds(15);
        _configuration = configuration;
        _updateIntervalSeconds = _configuration.GetValue<int>("StockService:UpdateIntervalSeconds", 30);
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
                            await ExecuteRequest(stoppingToken, symbol);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error fetching Finnhub quote for {Symbol}", symbol);
                        }

                        await Task.Delay(1000, stoppingToken); // Respect Finnhub's 30 requests/second limit
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(_updateIntervalSeconds), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Finnhub update loop");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }

    private async Task ExecuteRequest(CancellationToken stoppingToken, string symbol)
    {
        var url = $"{_configuration["ApiUrls:Finnhub"]}/quote?symbol={symbol}&token={Constants.FinnhubKey}";
        var response = await _httpClient.GetFromJsonAsync<FinnhubQuote>(url, stoppingToken);

        if (response?.CurrentPrice > 0)
        {
            await _hubContext.Clients.All.SendAsync(
                "ReceiveStockUpdate",
                symbol,
                (decimal)response.CurrentPrice,
                cancellationToken: stoppingToken);
        }
        else
        {
            _logger.LogWarning("Invalid price received for {Symbol}: {Price}", symbol, response?.CurrentPrice);
        }
    }
}