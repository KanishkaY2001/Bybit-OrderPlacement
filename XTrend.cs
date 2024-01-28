namespace TradingBot
{
    public class XTrend
    {  
        private decimal? next_trend { get; set; } = null;
        private decimal? trend { get; set; } = null;
        private decimal? low_max { get; set; } = null;
        private decimal? high_min { get; set; } = null;
        private decimal? line_HT { get; set; } = null;
        private decimal? sum { get; set; } = null;

        public decimal EMA(decimal close, int period = 3)
        {
            decimal alpha = 2m / (period + 1m);
            var src = close;
            var prevSum = sum;
            sum = prevSum == null ? src : alpha * src + (1 - alpha) * prevSum;
            return (decimal)sum;
        }

        public static decimal CalculateSMA(List<decimal> prices) {
            if (prices.Count < 2) throw new ArgumentException("Not enough data to calculate SMA.");

            decimal sum = 0m;
            foreach (var price in prices) {
                sum += price;
            }

            return sum / prices.Count;
        }

        public static decimal CalculateATR(List<Dictionary<string, decimal>> klines, int period = 10) {
            if (klines.Count < period) throw new ArgumentException("Not enough data to calculate ATR.");

            var trueRanges = new List<decimal>();
            for (int i = 1; i < klines.Count; i++) {
                var currentKline = klines[i];
                var previousKline = klines[i - 1];

                var range1 = currentKline["high"] - currentKline["low"];
                var range2 = Math.Abs(currentKline["high"] - previousKline["close"]);
                var range3 = Math.Abs(currentKline["low"] - previousKline["close"]);

                var trueRange = Math.Max(range1, Math.Max(range2, range3));
                trueRanges.Add(trueRange);
            }

            // Calculate the average of the true ranges
            return trueRanges.Skip(trueRanges.Count - period).Take(period).Average();
        }

        /**
        public decimal ATR(int length)
        {
            var high1 = high;
            var close1 = close;
            var trueRange = high1 == null ? high-low : Math.Max(Math.Max(high - low, Math.Abs(high - close1)), Math.Abs(low - close1));
        }
        **/

        public XTrend() {}

        public string Decision(List<Dictionary<string, decimal>> klineList)
        {
            // ignore 0th
            klineList.RemoveAt(0);
            klineList.Reverse();
            var count = klineList.Count();

            // lowest low
            List<decimal> lows = new List<decimal> { klineList[count - 3]["low"], klineList[count - 2]["low"], klineList[count - 1]["low"] };
            decimal lowest_low = lows.Min();
            decimal ma_low = EMA(klineList[count - 1]["low"]);

            List<decimal> highs = new List<decimal> { klineList[count - 2]["high"], klineList[count - 1]["high"] };
            decimal highest_high = highs.Max();
            var ma_high = CalculateSMA(highs);

            next_trend = next_trend == null ? 0m : next_trend;
            var prevTrend = trend;
            trend = trend == null ? 0m : trend;
            low_max = low_max == null ? klineList[count - 2]["low"] : low_max;
            high_min = high_min == null ? klineList[count - 2]["high"] : high_min;

            if (next_trend == 1)
            {
                low_max = Math.Max((decimal)low_max, lowest_low);
            }
            if (ma_high < low_max && klineList[count - 1]["close"] < klineList[count - 2]["low"])
            {
                trend = 1;
                next_trend = 0;
                high_min = highest_high;
            }

            if (next_trend == 0)
            {
                high_min = Math.Min((decimal)high_min, highest_high);
            }
            if (ma_low > high_min && klineList[count - 1]["close"] > klineList[count - 2]["high"])
            {
                trend = 0;
                next_trend = 1;
                low_max = lowest_low;
            }

            var prevLine_HT = line_HT;
            line_HT = line_HT == null? klineList[count - 1]["close"] : line_HT;
            if (prevLine_HT == null) prevLine_HT = line_HT;
            
            if (prevTrend == 0)
            {
                line_HT = Math.Max((decimal)low_max, (decimal)prevLine_HT);
            }
            if (prevTrend == 1)
            {
                line_HT = Math.Min((decimal)high_min, (decimal)prevLine_HT);
            }

            if (trend != prevTrend)
            {
                
                if (trend == 1)
                {
                    return "sell";
                }
                if (trend == -1)
                {
                    return "buy";
                }
            }
            return "hold";
        }
    }
}