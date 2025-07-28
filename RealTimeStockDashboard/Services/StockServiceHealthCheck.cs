using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace RealTimeStockDashboard.Services;

public class StockServiceHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var status = StockHubState.HasConnectedClient
            ? HealthStatus.Healthy
            : HealthStatus.Degraded;

        var data = new Dictionary<string, object>
        {
            { "connected_clients", StockHubState.ConnectedClientCount },
            { "last_update", StockHubState.LastUpdateTime }
        };

        return Task.FromResult(new HealthCheckResult(
            status,
            description: status == HealthStatus.Healthy
                ? "Stock service is healthy and clients are connected"
                : "Waiting for client connections",
            data: data));
    }
}