using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace RealTimeStockDashboard;

public static class StockHubState
{
    public static bool HasConnectedClient => ConnectedClientCount > 0;
    public static int ConnectedClientCount { get; private set; }
    public static DateTime LastUpdateTime { get; private set; } = DateTime.UtcNow;

    public static void ClientConnected()
    {
        ConnectedClientCount++;
        LastUpdateTime = DateTime.UtcNow;
    }

    public static void ClientDisconnected()
    {
        ConnectedClientCount = Math.Max(0, ConnectedClientCount - 1);
    }

    public static void UpdateReceived()
    {
        LastUpdateTime = DateTime.UtcNow;
    }
}

public class StockHub : Hub
{
    /*
     * No methods needed in this class unless clients will call something on the hub. For ex.:
     *  - a user clicks a button to manually trigger an update
     *  - there is a bidirectional communication (clients -> server)
     */
    public override Task OnConnectedAsync()
    {
        StockHubState.ClientConnected();
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        StockHubState.ClientDisconnected();
        return base.OnDisconnectedAsync(exception);
    }

    public async Task SendStockUpdate(string symbol, decimal price)
    {
        StockHubState.UpdateReceived();
        await Clients.All.SendAsync("ReceiveStockUpdate", symbol, price);
    }
}