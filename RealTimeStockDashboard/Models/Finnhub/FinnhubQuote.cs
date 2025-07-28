using System.Text.Json.Serialization;

namespace RealTimeStockDashboard.Models.Finnhub;

public class FinnhubQuote
{
    [JsonPropertyName("c")]
    public double CurrentPrice { get; set; }

    [JsonPropertyName("h")]
    public double HighOfDay { get; set; }

    [JsonPropertyName("l")]
    public double LowOfDay { get; set; }

    [JsonPropertyName("o")]
    public double OpenPrice { get; set; }

    [JsonPropertyName("pc")]
    public double PreviousClose { get; set; }
}