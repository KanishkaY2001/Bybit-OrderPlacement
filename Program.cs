using Newtonsoft.Json;

using bybit.net.api.ApiServiceImp;
using bybit.net.api.Models.Trade;
using bybit.net.api.Models;
using bybit.net.api;

namespace TradingBot
{
    class BybitSystem
    {
        public Settings settings { get; set; }
        public Dictionary<string, OrderInfo> orderDict { get; set; }
        private string apiKey { get; set; }
        private string apiSecret { get; set; }
        BybitTradeService tradeService { get; set; }

        public BybitSystem(Settings _settings, string key, string secret)
        {
            settings = _settings;
            orderDict = new Dictionary<string, OrderInfo>();
            apiKey = key;
            apiSecret = secret;
            tradeService = new(apiKey: apiKey, apiSecret: apiSecret, url: BybitConstants.HTTP_TESTNET_URL);
        }

        private void AddOrder(string id, string symbol, Side side, string qty)
        {
            orderDict.Add
            (
                id,
                new OrderInfo
                (
                    settings.category,
                    symbol,
                    side,
                    settings.orderType,
                    qty,
                    settings.timeInForce
                )
            );
        }

        public async Task<bool> PlaceOrder(string symbol, string qty)
        {
            var side = Side.BUY;
            var orderObject = await tradeService.PlaceOrder
            (
                category: Category.LINEAR, 
                symbol: symbol, 
                side: side,
                orderType: OrderType.MARKET, 
                qty: qty, 
                timeInForce: TimeInForce.GTC
            );

            string id = JsonConvert.DeserializeObject<BybitResponse>(orderObject!.ToString())!.result.orderId;
            Console.WriteLine($"\nOrder Placed ({DateTime.Now})\nSymbol: {symbol}\nQuantity: {qty}\nOrderId: {id}");
            AddOrder(id, symbol, side, qty);

            return true;
        }

        public async Task<bool> CloseOrder(string symbol, string qty)
        {
            var side = Side.SELL;
            var orderObject = await tradeService.PlaceOrder
            (
                category: Category.LINEAR, 
                symbol: symbol, 
                side: side,
                orderType: OrderType.MARKET, 
                qty: qty, 
                timeInForce: TimeInForce.GTC
            );

            string id = JsonConvert.DeserializeObject<BybitResponse>(orderObject!.ToString())!.result.orderId;
            Console.WriteLine($"\nOrder Closed ({DateTime.Now})\nSymbol: {symbol}\nQuantity: {qty}\nOrderId: {id}");
            AddOrder(id, symbol, side, qty);

            return true;
        }

        public async Task<bool> CancelOrder(string id)
        {
            OrderInfo orderInfo = orderDict[id];
            Category category = orderInfo.category;
            string symbol = orderInfo.symbol;

            var orderObject = await tradeService.CancelOrder
            (
                orderId: id,
                category: category, 
                symbol: symbol
            );

            Console.WriteLine(orderObject);
            Console.WriteLine($"\nOrder Cancelled ({DateTime.Now})\nSymbol: {symbol}\nOrderId: {id}");
            
            return true;
        }

        public async Task<bool> PlaceStopLossOrder(string symbol, string qty, string slPrice)
        {
            var side = Side.BUY;
            var orderObject = await tradeService.PlaceOrder
            (
                category: Category.LINEAR, 
                symbol: symbol, 
                side: side,
                orderType: OrderType.MARKET, 
                qty: qty, 
                stopLoss: slPrice,
                timeInForce: TimeInForce.GTC
            );

            string id = JsonConvert.DeserializeObject<BybitResponse>(orderObject!.ToString())!.result.orderId;
            Console.WriteLine($"\nSL Order Placed ({DateTime.Now})\nSymbol: {symbol}\nQuantity: {qty}\nStopLoss: {slPrice}\nOrderId: {id}");
            AddOrder(id, symbol, side, qty);

            return true;
        }

        /**
        public async Task<bool> UpdateStopLossOrder(string id)
        {
            // Not possible with API
            return true;
        }
        **/
    }

    class Settings
    {
        public Category category { get; set; }
        public OrderType orderType { get; set; }
        public TimeInForce timeInForce { get; set; }
        public Settings()
        {
            category = Category.LINEAR;
            orderType = OrderType.MARKET;
            timeInForce = TimeInForce.GTC;
        }
    }
    
    class System
    {
        public static async Task Main(string[] args)
        {
            // Configure default and API settings
            var settings = new Settings();
            string[] lines = File.ReadAllLines("apiConfig");
            var sys = new BybitSystem(settings, lines[0], lines[1]);

            // Commence trading
            await sys.PlaceStopLossOrder("BLZUSDT", "10", "0.25");
        }
    }
}