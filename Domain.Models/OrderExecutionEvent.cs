using Domain.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class OrderExecutionEvent
    {
        [JsonPropertyName("event_id")]
        public string EventId { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("order_id")]
        public string OrderId { get; set; }

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("executed_quantity")]
        public decimal ExecutedQuantity { get; set; }

        [JsonPropertyName("executed_price")]
        public decimal ExecutedPrice { get; set; }

        [JsonPropertyName("remaining_quantity")]
        public decimal RemainingQuantity { get; set; }

        [JsonPropertyName("status")]
        public OrderStatus Status { get; set; }

        [JsonPropertyName("execution_id")]
        public string ExecutionId { get; set; } = Guid.NewGuid().ToString();
    }
}
