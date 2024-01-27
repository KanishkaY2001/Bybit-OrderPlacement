using System.Text;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace TradingBot
{
    class Settings
    {
        public string category { get; set; }
        public string orderType { get; set; }
        public string timeInForce { get; set; }
        public string net { get; set; }
        public Settings(string _net)
        {
            category = "linear";
            orderType = "market";
            timeInForce = "GTC";
            net = _net;
        }
    }

    class BybitTime
    {
        public long time { get; set; }
    }

    class BybitHttp
    {
        public HttpClient client { get; set; }
        private Dictionary<string, string> networks { get; set; }
        private string apiKey { get; set; }
        private string apiSecret { get; set; }
        private string recvWindow { get; set; }

        public BybitHttp(string key, string secret)
        {
            client = new HttpClient();
            apiKey = key;
            apiSecret = secret;
            recvWindow = "5000";
            networks = new Dictionary<string, string>
            {
                {"testnet", "https://api-testnet.bybit.com"},
                {"mainnet", "https://api.bybit.com"}
            };
        }

        public async Task<string> GetTimestamp()
        {
            string timeUri = "https://api-testnet.bybit.com/v5/market/time";
            try
            {
                using HttpResponseMessage response = await client.GetAsync(timeUri);
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<BybitTime>(json!.ToString())!.time.ToString();
            }
            catch (HttpRequestException e)
            {
                return e.ToString();
            }
        }

        private string GeneratePostSignature(Dictionary<string, object> parameters, string _timeStamp)
        {
            string jsonPayload = JsonConvert.SerializeObject(parameters);
            string rawData = _timeStamp + apiKey + recvWindow + jsonPayload;
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(apiSecret));
            var signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return BitConverter.ToString(signature).Replace("-", "").ToLower();
        }

        private string GenerateGetSignature(Dictionary<string, object> parameters, string _timeStamp)
        {
            string queryString = GenerateQueryString(parameters);
            string rawData = _timeStamp + apiKey + recvWindow + queryString;
            return ComputeSignature(rawData);
        }

        private string ComputeSignature(string data)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(apiSecret));
            byte[] signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(signature).Replace("-", "").ToLower();
        }

        private string GenerateQueryString(Dictionary<string, object> parameters)
        {
            return string.Join("&", parameters.Select(p => $"{p.Key}={p.Value}"));
        }

        public async Task<string> SEND(HttpMethod method, string requestUri, Dictionary<string, object> parameters, Settings settings)
        {
            string net = networks[settings.net];
            string timestamp = await GetTimestamp();
            string jsonPayload = JsonConvert.SerializeObject(parameters);
            string queryString = method == HttpMethod.Post ? "" : GenerateQueryString(parameters);
            string signature = method == HttpMethod.Post ? GeneratePostSignature(parameters, timestamp)
                                                         : GenerateGetSignature(parameters, timestamp);
            string finalUri = method == HttpMethod.Post ? $"{net}{requestUri}" : $"{net}{requestUri}?{queryString}";

            HttpRequestMessage request = new(method, finalUri)
            {
                Content = method == HttpMethod.Post ? new StringContent(jsonPayload, Encoding.UTF8, "application/json") : null
            };
            request.Headers.Add("X-BAPI-API-KEY", apiKey);
            request.Headers.Add("X-BAPI-SIGN", signature);
            request.Headers.Add("X-BAPI-SIGN-TYPE", "2");
            request.Headers.Add("X-BAPI-TIMESTAMP", timestamp);
            request.Headers.Add("X-BAPI-RECV-WINDOW", recvWindow);

            try
            {
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                string responseJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseJson);
                return responseJson;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(e);
                return e.ToString();
            }
        }
    }

    class BybitTrade
    {   
        private BybitHttp http { get; set; }
        public Settings settings { get; set; }
        
        public BybitTrade(string key, string secret, string net)
        {
            settings = new Settings(net);
            http = new BybitHttp(key, secret);
        }

        private async Task<bool> ProcessHttp(HttpMethod method, Dictionary<string, object> parameters, string requestUri)
        {
            return await http.SEND(method, requestUri, parameters, settings) != string.Empty ? true : false;
        }

        public async Task<bool> PlaceOrder(string symbol, string quantity, string side)
        {
            var parameters = new Dictionary<string, object>
            {
                {"category", settings.category},
                {"symbol", symbol},
                {"isLeverage", 1},
                {"side", side},
                {"orderType", settings.orderType},
                {"qty", quantity},
                {"positionIdx", 1},
                {"timeInForce", settings.timeInForce},
            };
            return await ProcessHttp(HttpMethod.Post, parameters, "/v5/order/create");
        }

        public async Task<bool> BuyOrder(string symbol, string quantity)
        {
            return await PlaceOrder(symbol, quantity, "Buy");
        }

        public async Task<bool> SellOrder(string symbol, string quantity)
        {
            return await PlaceOrder(symbol, quantity, "Sell");
        }

        public async Task<bool> Position(string symbol)
        {
            var parameters = new Dictionary<string, object>
            {
                {"category", settings.category},
                {"symbol", symbol}
            };
            return await ProcessHttp(HttpMethod.Get, parameters, "/v5/position/list");
        }

        public async Task<bool> SetLeverage(string symbol, string buyLeverage, string sellLeverage)
        {
            var parameters = new Dictionary<string, object>
            {
                {"category", settings.category},
                {"symbol", symbol},
                {"buyLeverage", buyLeverage},
                {"sellLeverage", sellLeverage}
            };
            return await ProcessHttp(HttpMethod.Post, parameters, "/v5/position/set-leverage");
        }

        public async Task<bool> SetTradingStop(string symbol, string takeProfit, string stopLoss)
        {
            var parameters = new Dictionary<string, object>
            {
                {"category", settings.category},
                {"symbol", symbol},
                {"takeProfit", takeProfit},
                {"stopLoss", stopLoss},
                {"positionIdx", 0}
            };
            return await ProcessHttp(HttpMethod.Post, parameters, "/v5/position/trading-stop");
        }
    }
}