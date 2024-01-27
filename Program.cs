namespace TradingBot
{
    class System
    {
        public static async Task Main(string[] args)
        {
            var bybitTrade = new BybitTrade
            (
                "yVPtPiuyLG5dIEqYDp", // ApiKey
                "GSvQ7BS73yA3gIOgT6DG1JfkwOXxV6f8OSfY", // ApiSecret
                "mainnet" // "testnet" or "mainnet"
            );

            // Place "buy" order
            // Purchase 50 units of BLZUSDT
            //await bybitTrade.BuyOrder("BLZUSDT", "5");
            
            // Place "sell" order
            // Sell 8 units of BLZUSDT
            await bybitTrade.SellOrder("BLZUSDT", "5");
            
            /**
            // Check position
            // Check current position for BLZUSDT
            await bybitTrade.Position("BLZUSDT");

            // Set Leverage
            // For the current 27 units of BLZUSDT, SET the buy leverage to 6 and sell leverage to 4
            await bybitTrade.SetLeverage("BLZUSDT", "6", "4");

            // Update Leverage
            // For the current 27 units of BLZUSDT, UPDATE the buy leverage to 16 and sell leverage to 14
            await bybitTrade.SetLeverage("BLZUSDT", "16", "14");

            // Set trading stop
            // For the current 27 units of BLZUSDT, set TP at 5, and SL at 0
            await bybitTrade.SetTradingStop("BLZUSDT", "5", "0");

            // Update trading stop
            // For the current 27 units of BLZUSDT, UPDATE the tp to 10, and SL to 0.1
            await bybitTrade.SetTradingStop("BLZUSDT", "10", "0.1");
            **/
        }
    }
}