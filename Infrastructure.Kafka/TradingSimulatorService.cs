using Domain.Core.Interfaces;
using Domain.Models;
using Domain.Models.Enums;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Kafka
{
    public class TradingSimulatorService : BackgroundService
    {
        private readonly IKafkaProducerService _kafkaProducer;
        private readonly ILogger<TradingSimulatorService> _logger;
        private readonly Random _random = new Random();
        private readonly string[] _symbols = { "AAPL", "GOOGL", "MSFT", "AMZN", "TSLA", "META", "NVDA" };

        public TradingSimulatorService(
            IKafkaProducerService kafkaProducer,
            ILogger<TradingSimulatorService> logger)
        {
            _kafkaProducer = kafkaProducer;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Trading Simulator Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Generate random trade event
                    var tradeEvent = GenerateTradeEvent();
                    await _kafkaProducer.PublishTradeEventAsync(tradeEvent);

                    // Simulate order execution
                    if (_random.NextDouble() > 0.3) // 70% execution rate
                    {
                        var execution = GenerateExecutionEvent(tradeEvent);
                        await _kafkaProducer.PublishOrderExecutionAsync(execution);
                    }

                    // Generate market data
                    var marketData = GenerateMarketData();
                    await _kafkaProducer.PublishMarketDataAsync(marketData);

                    await Task.Delay(TimeSpan.FromMilliseconds(500), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in trading simulator");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        private TradeEvent GenerateTradeEvent()
        {
            var symbol = _symbols[_random.Next(_symbols.Length)];
            var side = _random.Next(2) == 0 ? OrderSide.Buy : OrderSide.Sell;
            var price = 100 + (decimal)_random.NextDouble() * 100;
            var quantity = _random.Next(10, 1000);

            return new TradeEvent
            {
                Symbol = symbol,
                OrderType = OrderType.Limit,
                Side = side,
                Price = Math.Round(price, 2),
                Quantity = quantity,
                UserId = $"user_{_random.Next(1, 100)}",
                OrderId = Guid.NewGuid().ToString()
            };
        }

        private OrderExecutionEvent GenerateExecutionEvent(TradeEvent trade)
        {
            var executedQty = trade.Quantity * (decimal)(0.5 + _random.NextDouble() * 0.5);

            return new OrderExecutionEvent
            {
                OrderId = trade.OrderId,
                Symbol = trade.Symbol,
                ExecutedQuantity = Math.Round(executedQty, 2),
                ExecutedPrice = trade.Price,
                RemainingQuantity = trade.Quantity - executedQty,
                Status = executedQty >= trade.Quantity ? OrderStatus.Filled : OrderStatus.PartiallyFilled
            };
        }

        private MarketDataEvent GenerateMarketData()
        {
            var symbol = _symbols[_random.Next(_symbols.Length)];
            var midPrice = 100 + (decimal)_random.NextDouble() * 100;
            var spread = (decimal)_random.NextDouble() * 0.5m;

            return new MarketDataEvent
            {
                Symbol = symbol,
                BidPrice = Math.Round(midPrice - spread / 2, 2),
                AskPrice = Math.Round(midPrice + spread / 2, 2),
                LastPrice = Math.Round(midPrice, 2),
                Volume = _random.Next(10000, 1000000),
                BidSize = _random.Next(100, 10000),
                AskSize = _random.Next(100, 10000)
            };
        }
    }
}