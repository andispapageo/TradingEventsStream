using Confluent.Kafka;
using Domain.Models;
using Domain.Models.Enums;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Kafka
{
    public class TradeEventConsumerService : BackgroundService
    {
        private readonly ILogger<TradeEventConsumerService> _logger;
        private IConsumer<string, string> _consumer;

        public TradeEventConsumerService(ILogger<TradeEventConsumerService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "trading-processor",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false, // Manual commit for control
                SessionTimeoutMs = 6000,
                MaxPollIntervalMs = 300000
            };

            _consumer = new ConsumerBuilder<string, string>(config)
                .SetErrorHandler((_, e) => _logger.LogError($"Error: {e.Reason}"))
                .Build();

            _consumer.Subscribe("trading.events");

            _logger.LogInformation("Trade Event Consumer started");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(stoppingToken);

                        if (consumeResult?.Message != null)
                        {
                            await ProcessTradeEvent(consumeResult.Message);

                            // Commit offset after successful processing
                            _consumer.Commit(consumeResult);
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, $"Consume error: {ex.Error.Reason}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Consumer cancelled");
            }
            finally
            {
                _consumer.Close();
                _consumer.Dispose();
            }
        }

        private async Task ProcessTradeEvent(Message<string, string> message)
        {
            try
            {
                var tradeEvent = JsonSerializer.Deserialize<TradeEvent>(message.Value);

                _logger.LogInformation(
                    $"Processing Trade: {tradeEvent.Symbol} - {tradeEvent.Side} " +
                    $"{tradeEvent.Quantity} @ {tradeEvent.Price:C}");

                // Business logic
                await ValidateOrder(tradeEvent);
                await CheckRiskLimits(tradeEvent);
                await UpdatePositions(tradeEvent);

                // Simulate processing delay
                await Task.Delay(100);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize trade event");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing trade event");
                throw; // Don't commit on error
            }
        }

        private async Task ValidateOrder(TradeEvent trade)
        {
            // Validate order parameters
            if (trade.Quantity <= 0)
            {
                _logger.LogWarning($"Invalid quantity for order {trade.OrderId}");
                return;
            }

            if (trade.Price <= 0 && trade.OrderType != OrderType.Market)
            {
                _logger.LogWarning($"Invalid price for order {trade.OrderId}");
                return;
            }

            await Task.CompletedTask;
        }

        private async Task CheckRiskLimits(TradeEvent trade)
        {
            var tradeValue = trade.Quantity * trade.Price;

            if (tradeValue > 100000)
            {
                _logger.LogWarning(
                    $"⚠️ Large trade detected: {trade.OrderId} - Value: {tradeValue:C}");
            }

            // Additional risk checks would go here
            await Task.CompletedTask;
        }

        private async Task UpdatePositions(TradeEvent trade)
        {
            // Update position tracking
            _logger.LogInformation($"Updating positions for {trade.Symbol}");
            await Task.CompletedTask;
        }
    }
}
