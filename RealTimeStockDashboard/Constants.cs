using System;
using System.Collections.Generic;

namespace RealTimeStockDashboard;

public static class Constants
{
    // US Stocks
    public const string IBM = "IBM";
    public const string AAPL = "AAPL";
    public const string MSFT = "MSFT";
    public const string GOOGL = "GOOGL";
    public const string AMZN = "AMZN";
    public const string META = "META";
    public const string TSLA = "TSLA";

    // Europe Stocks
    public const string DAX = "DAX";
    public const string SAP = "SAP";
    public const string ASML = "ASML";
    public const string AIR = "AIR";
    public const string HSBC = "HSBC";
    public const string BP = "BP";
    public const string TSM = "TSM";

    // Asia Stocks
    public const string TYO = "TYO";
    public const string BYD = "BYD";
    public const string BABA = "BABA";
    public const string TCEHY = "TCEHY";
    public const string SONY = "SONY";
    public const string MFG = "MFG";
    public const string PDD = "PDD";

    public static readonly Dictionary<string, string[]> RegionSymbols = new()
    {
        ["US"] = new[] { IBM, AAPL, MSFT, GOOGL, AMZN, META, TSLA },
        ["Europe"] = new[] { DAX, SAP, ASML, AIR, HSBC, BP, TSM },
        ["Asia"] = new[] { TYO, BYD, BABA, TCEHY, SONY, MFG, PDD }
    };

    public static readonly Dictionary<string, decimal> MockPrices = new()
    {
        // US
        [IBM] = 21800m,
        [AAPL] = 195.32m,
        [MSFT] = 340.12m,
        [GOOGL] = 142.75m,
        [AMZN] = 129.58m,
        [META] = 306.89m,
        [TSLA] = 211.07m,

        // Europe
        [DAX] = 18450.56m,
        [SAP] = 175.20m,
        [ASML] = 950.75m,
        [AIR] = 825.50m,
        [HSBC] = 42.18m,
        [BP] = 38.92m,
        [TSM] = 140.65m,

        // Asia
        [TYO] = 38250.75m,
        [BYD] = 16520.30m,
        [BABA] = 78.90m,
        [TCEHY] = 45.60m,
        [SONY] = 1420.00m,
        [MFG] = 8.75m,
        [PDD] = 132.40m
    };

    public static string FinnhubKey =>
        Environment.GetEnvironmentVariable("FINNHUB_API_KEY")
        ?? throw new Exception("FINNHUB_API_KEY not set");

    public static string AlphaVantageKey =>
        Environment.GetEnvironmentVariable("ALPHAVANTAGE_API_KEY")
        ?? throw new Exception("ALPHAVANTAGE_API_KEY not set");

    public static decimal DefaultMockPrice => 10m;
}