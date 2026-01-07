using Application.Common.DTOs;
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

        public TradingController(
            IKafkaProducerService kafkaProducer,
            ILogger<TradingController> logger)
        {
            _kafkaProducer = kafkaProducer;
            _logger = logger;
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
                    Message = "Failed to publish order event"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting order");
                return StatusCode(500, "Internal server error");
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
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                service = "Trading Events API"
            });
        }
    }
}
