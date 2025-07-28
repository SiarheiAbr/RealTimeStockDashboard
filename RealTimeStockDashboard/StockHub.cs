using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace RealTimeStockDashboard;

public static class StockHubState
{
    public static bool HasConnectedClient;
}

public class StockHub : Hub
{
    /*
     * No methods needed in this class unless clients will call something on the hub. For ex.:
     *  - a user clicks a button to manually trigger an update
     *  - there is a bidirectional communication (clients -> server)
     */
    public async Task SendStockUpdate(string symbol, decimal price)
    {
        await Clients.All.SendAsync("ReceiveStockUpdate", symbol, price);
    }

    public override Task OnConnectedAsync()
    {
        StockHubState.HasConnectedClient = true;
        return base.OnConnectedAsync();
    }
}