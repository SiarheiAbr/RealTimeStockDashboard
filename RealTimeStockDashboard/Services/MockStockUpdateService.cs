using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;

namespace RealTimeStockDashboard.Services;

// Mock local service
public class MockStockUpdateService : BackgroundService
{
    private readonly IHubContext<StockHub> _hubContext;
    private readonly Random _rnd = new();

    public MockStockUpdateService(IHubContext<StockHub> hubContext)
    {
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!StockHubState.HasConnectedClient && !stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(500, stoppingToken);
        }

        var prices = Constants.MockPrices;

        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var region in Constants.RegionSymbols)
            {
                foreach (var symbol in region.Value)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", symbol, prices[symbol],
                        cancellationToken: stoppingToken);
                }
            }

            await Task.Delay(5000, stoppingToken);

            // simulate stock prices change
            foreach (var symbol in Constants.MockPrices.Keys)
            {
                var change = (decimal)(_rnd.NextDouble() - 0.5) * 100;
                var newPrice = prices[symbol] + change;

                // Ensure the price doesn't go below a realistic floor (e.g., $10)
                prices[symbol] = Math.Max(newPrice, Constants.DefaultMockPrice);
            }
        }
    }
}