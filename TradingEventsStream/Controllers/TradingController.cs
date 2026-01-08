using Application.Common.DTOs;
using Confluent.Kafka;
using Domain.Core.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace TradingEventsStream.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TradingController : ControllerBase
    {
        private readonly IKafkaProducerService _kafkaProducer;
        private readonly ILogger<TradingController> _logger;
        private readonly IConfiguration _configuration;

        public TradingController(
            IKafkaProducerService kafkaProducer,
            ILogger<TradingController> logger,
            IConfiguration configuration)
        {
            _kafkaProducer = kafkaProducer;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("orders")]
        [ProducesResponseType(typeof(TradeResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SubmitOrder([FromBody] TradeRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var tradeEvent = new TradeEvent
                {
                    Symbol = request.Symbol.ToUpper(),
                    OrderType = request.OrderType,
                    Side = request.Side,
                    Quantity = request.Quantity,
                    Price = request.Price,
                    UserId = request.UserId,
                    OrderId = Guid.NewGuid().ToString()
                };

                var success = await _kafkaProducer.PublishTradeEventAsync(tradeEvent);

                if (success)
                {
                    _logger.LogInformation(
                        $"Order submitted: {tradeEvent.OrderId} - {tradeEvent.Symbol}");

                    return Ok(new TradeResponse
                    {
                        OrderId = tradeEvent.OrderId,
                        Status = "Accepted",
                        Message = "Order has been submitted for processing",
                        Timestamp = DateTime.UtcNow
                    });
                }

                return StatusCode(500, new TradeResponse
                {
                    Status = "Failed",
                    Message = "Failed to publish order event. Kafka may be unavailable."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting order");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        [HttpPost("market-data")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> PublishMarketData([FromBody] MarketDataRequest request)
        {
            try
            {
                var marketData = new MarketDataEvent
                {
                    Symbol = request.Symbol.ToUpper(),
                    BidPrice = request.BidPrice,
                    AskPrice = request.AskPrice,
                    LastPrice = request.LastPrice,
                    Volume = request.Volume,
                    BidSize = request.BidSize,
                    AskSize = request.AskSize
                };

                await _kafkaProducer.PublishMarketDataAsync(marketData);
                return Ok(new { message = "Market data published" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing market data");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            try
            {
                var bootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

                // Test Kafka connection
                var config = new AdminClientConfig { BootstrapServers = bootstrapServers };
                using var adminClient = new AdminClientBuilder(config).Build();

                var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(5));
                var brokerCount = metadata.Brokers.Count;

                if (brokerCount == 0)
                {
                    return StatusCode(503, new
                    {
                        status = "Unhealthy",
                        timestamp = DateTime.UtcNow,
                        service = "Trading Events API",
                        kafka = new
                        {
                            status = "Disconnected",
                            bootstrapServers = bootstrapServers,
                            brokers = 0
                        }
                    });
                }

                return Ok(new
                {
                    status = "Healthy",
                    timestamp = DateTime.UtcNow,
                    service = "Trading Events API",
                    kafka = new
                    {
                        status = "Connected",
                        bootstrapServers = bootstrapServers,
                        brokers = brokerCount,
                        brokerDetails = metadata.Brokers.Select(b => new
                        {
                            id = b.BrokerId,
                            host = b.Host,
                            port = b.Port
                        })
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(503, new
                {
                    status = "Unhealthy",
                    timestamp = DateTime.UtcNow,
                    service = "Trading Events API",
                    error = ex.Message
                });
            }
        }

        [HttpGet("kafka/topics")]
        public IActionResult GetTopics()
        {
            try
            {
                var bootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
                var config = new AdminClientConfig { BootstrapServers = bootstrapServers };

                using var adminClient = new AdminClientBuilder(config).Build();
                var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(5));

                return Ok(new
                {
                    topics = metadata.Topics.Select(t => new
                    {
                        name = t.Topic,
                        partitions = t.Partitions.Count,
                        error = t.Error?.Reason
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve topics");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

}
