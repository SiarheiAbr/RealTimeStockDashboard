namespace RealTimeStockDashboard.Models.AlphaVantage;

public class AlphaVantageResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("Global Quote")]
    public GlobalQuote GlobalQuote { get; set; }
}