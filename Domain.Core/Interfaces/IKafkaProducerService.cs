using Domain.Models;

namespace Domain.Core.Interfaces
{
    public interface IKafkaProducerService
    {
        Task<bool> PublishTradeEventAsync(TradeEvent tradeEvent);
        Task<bool> PublishOrderExecutionAsync(OrderExecutionEvent executionEvent);
        Task<bool> PublishMarketDataAsync(MarketDataEvent marketData);
    }
}
