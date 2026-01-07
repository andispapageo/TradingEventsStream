using Confluent.Kafka;
using Domain.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Kafka
{
    public class OrderExecutionConsumerService : BackgroundService
    {
        private readonly ILogger<OrderExecutionConsumerService> _logger;
        private IConsumer<string, string> _consumer;

        public OrderExecutionConsumerService(ILogger<OrderExecutionConsumerService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "execution-processor",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            _consumer = new ConsumerBuilder<string, string>(config).Build();
            _consumer.Subscribe("order.executions");

            _logger.LogInformation("Order Execution Consumer started");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = _consumer.Consume(stoppingToken);

                        if (result?.Message != null)
                        {
                            var execution = JsonSerializer.Deserialize<OrderExecutionEvent>(
                                result.Message.Value);

                            _logger.LogInformation(
                                $"Execution: {execution.Symbol} - " +
                                $"Filled {execution.ExecutedQuantity} @ {execution.ExecutedPrice:C} - " +
                                $"Status: {execution.Status}");

                            // Send notifications, update order book, etc.
                            await Task.Delay(50, stoppingToken);

                            _consumer.Commit(result);
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Consume error");
                    }
                }
            }
            finally
            {
                _consumer.Close();
                _consumer.Dispose();
            }
        }
    }
}
