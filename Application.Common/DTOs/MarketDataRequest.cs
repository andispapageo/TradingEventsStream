using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.DTOs
{
    public class MarketDataRequest
    {
        public string Symbol { get; set; }
        public decimal BidPrice { get; set; }
        public decimal AskPrice { get; set; }
        public decimal LastPrice { get; set; }
        public decimal Volume { get; set; }
        public decimal BidSize { get; set; }
        public decimal AskSize { get; set; }
    }
}
