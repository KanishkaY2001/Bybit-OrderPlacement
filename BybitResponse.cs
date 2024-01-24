#nullable disable

namespace TradingBot
{
    public class Result
    {
        public string orderId { get; set; }
        public string orderLinkId { get; set; }
    }

    public class RetExtInfo
    {
    }

    public class BybitResponse
    {
        public int retCode { get; set; }
        public string retMsg { get; set; }
        public Result result { get; set; }
        public RetExtInfo retExtInfo { get; set; }
        public long time { get; set; }
    }
}