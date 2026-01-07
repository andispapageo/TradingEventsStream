using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class MarketDataEvent
    {
        [JsonPropertyName("event_id")]
        public string EventId { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("bid_price")]
        public decimal BidPrice { get; set; }

        [JsonPropertyName("ask_price")]
        public decimal AskPrice { get; set; }

        [JsonPropertyName("last_price")]
        public decimal LastPrice { get; set; }

        [JsonPropertyName("volume")]
        public decimal Volume { get; set; }

        [JsonPropertyName("bid_size")]
        public decimal BidSize { get; set; }

        [JsonPropertyName("ask_size")]
        public decimal AskSize { get; set; }
    }
}
