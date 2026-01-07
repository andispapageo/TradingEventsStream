using Confluent.Kafka;
using Domain.Core.Interfaces;
using Domain.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Kafka
{
    public class KafkaProducerService : IKafkaProducerService, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaProducerService> _logger;
        private readonly string _tradeEventsTopic = "trading.events";
        private readonly string _executionTopic = "order.executions";
        private readonly string _marketDataTopic = "market.data";

        public KafkaProducerService(ILogger<KafkaProducerService> logger)
        {
            _logger = logger;

            var config = new ProducerConfig
            {
                BootstrapServers = "localhost:9092",
                Acks = Acks.All, // Wait for all in-sync replicas
                EnableIdempotence = true, // Exactly-once semantics
                MaxInFlight = 5,
                MessageSendMaxRetries = 3,
                CompressionType = CompressionType.Snappy,
                LingerMs = 10, 
                BatchSize = 16384 
            };

            _producer = new ProducerBuilder<string, string>(config)
                .SetErrorHandler((_, e) => _logger.LogError($"Error: {e.Reason}"))
                .Build();
        }

        public async Task<bool> PublishTradeEventAsync(TradeEvent tradeEvent)
        {
            return await PublishEventAsync(_tradeEventsTopic, tradeEvent.Symbol, tradeEvent);
        }

        public async Task<bool> PublishOrderExecutionAsync(OrderExecutionEvent executionEvent)
        {
            return await PublishEventAsync(_executionTopic, executionEvent.Symbol, executionEvent);
        }

        public async Task<bool> PublishMarketDataAsync(MarketDataEvent marketData)
        {
            return await PublishEventAsync(_marketDataTopic, marketData.Symbol, marketData);
        }

        private async Task<bool> PublishEventAsync<T>(string topic, string key, T eventData)
        {
            try
            {
                var json = JsonSerializer.Serialize(eventData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var message = new Message<string, string>
                {
                    Key = key,
                    Value = json,
                    Timestamp = Timestamp.Default
                };

                var result = await _producer.ProduceAsync(topic, message);

                _logger.LogInformation(
                    $"Published to {topic}: Partition={result.Partition}, Offset={result.Offset}, Key={key}");

                return true;
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, $"Failed to publish to {topic}: {ex.Error.Reason}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error publishing to {topic}");
                return false;
            }
        }

        public void Dispose()
        {
            _producer?.Flush(TimeSpan.FromSeconds(10));
            _producer?.Dispose();
        }
    }
}
