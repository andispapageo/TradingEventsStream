using System.Text.Json.Serialization;

namespace Domain.Models
{
    public class TradingAnalytics
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("total_volume")]
        public decimal TotalVolume { get; set; }

        [JsonPropertyName("trade_count")]
        public int TradeCount { get; set; }

        [JsonPropertyName("avg_trade_size")]
        public decimal AvgTradeSize { get; set; }

        [JsonPropertyName("buy_volume")]
        public decimal BuyVolume { get; set; }

        [JsonPropertyName("sell_volume")]
        public decimal SellVolume { get; set; }

        [JsonPropertyName("vwap")]
        public decimal VWAP { get; set; } // Volume Weighted Average Price
    }
}
