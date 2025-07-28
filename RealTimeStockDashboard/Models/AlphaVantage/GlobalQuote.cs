using System.Text.Json.Serialization;

namespace RealTimeStockDashboard.Models.AlphaVantage;

public class GlobalQuote
{
    [JsonPropertyName("05. price")]
    public string Price { get; set; }
}