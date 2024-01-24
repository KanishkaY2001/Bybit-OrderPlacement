using bybit.net.api.Models;
using bybit.net.api.Models.Trade;

namespace TradingBot
{
    public class OrderInfo
    {
        public Category category { get; set; }
        public string symbol { get; set; }
        public Side side { get; set; }
        public OrderType orderType { get; set; }
        public string quantity { get; set; }
        public TimeInForce timeInForce { get; set; }

        public OrderInfo
        (
            Category _c, 
            string _sy, 
            Side _si, 
            OrderType _ot, 
            string _q, 
            TimeInForce _tif
        )
        {
            category = _c;
            symbol = _sy;
            side = _si;
            orderType = _ot;
            quantity = _q;
            timeInForce = _tif;
        }
    }
}