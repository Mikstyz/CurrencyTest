using ConnectorTest;
using RestSharp;
using Serilog;
using System.Text.Json;
using TestHQ;

namespace Service.Connector
{
    public class BConnector : ITestConnector
    {
        private readonly RestClient _client;

        public BConnector()
        {
            var options = new RestClientOptions("https://api-pub.bitfinex.com/v2")
            {
                ThrowOnAnyError = false
            };
            _client = new RestClient(options);
        }

        public async Task<bool> StatPlatform()
        {
            var request = new RestRequest("platform/status");
            request.AddHeader("accept", "application/json");

            RestResponse response = await _client.GetAsync(request);
            return response.IsSuccessful && response.Content == "[1]";
        }



        #region Rest

        public async Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount)
        {
            var request = new RestRequest($"https://api-pub.bitfinex.com/v2/trades/{pair}/hist?limit={maxCount}&sort=-1");
            request.AddHeader("accept", "application/json");

            RestResponse response = _client.Get(request);
            if (!response.IsSuccessful)
            {
                return null;
            }


            var json = JsonSerializer.Deserialize<List<List<decimal>>>(response.Content);
            if (json == null)
            {
                return null;
            }


            List<Trade> trades = new List<Trade>();

            foreach (var item in json)
            {
                if (item.Count < 4) continue;

                trades.Add(new Trade()
                {
                    Pair = pair,
                    Id = item[0].ToString(),
                    Time = DateTimeOffset.FromUnixTimeMilliseconds((long)item[1]),
                    Amount = item[2],
                    Price = item[3],
                    Side = item[2] > 0 ? "buy":"sell"
                });
            }

            return trades;
        }

        public async Task<IEnumerable<Candle>> GetCandleSeriesAsync(string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0)
        {
            Dictionary<int, string> TimeFrame = new Dictionary<int, string>()
            {
                { 60, "1m"},
                { 300, "5m"},
                { 900, "15m"},
                { 1800, "30m"},
                { 3600, "1h"},
                { 10800, "3h"},
                { 21600, "6h"},
                { 43200, "12h"},
                { 86400, "7D"},
                { 1209600, "14D"},
                { 2592000, "1M"}
            };

            var request = new RestRequest($"https://api-pub.bitfinex.com/v2/candles/trade:{TimeFrame[periodInSec]}:{pair}:a30:p2:p30/hist .");
            
            try
            {
                request.AddHeader("accept", "application/json");
                RestResponse response = _client.Get(request);

                var json = JsonSerializer.Deserialize<List<List<decimal>>>(response.Content);
                if (json == null)
                {
                    Log.Warning("полученный json пуст");
                }

                var candles = new List<Candle>();
                foreach (var item in json)
                {
                    if (item.Count < 6) continue;

                    candles.Add(new Candle
                    {
                        Pair = pair,
                        OpenTime = DateTimeOffset.FromUnixTimeMilliseconds((long)item[0]),
                        OpenPrice = item[1],
                        ClosePrice = item[2],
                        HighPrice = item[3],
                        LowPrice = item[4],
                        TotalVolume = item[5],
                        TotalPrice = item[5] * item[2]  //Не очень понял от куда мне барть TotalPrice (взял как итоговую цену сделки), взял как цену закрытия * объем

                        /*-----
                        
                        Или же можно взять TotalPrice как итог сделки это тогда уже

                        (закрытие * объем) - (цена открытия * объем)
                        (item[5] * item[2]) - (item[1]*item[5])

                        в данном случае если сделка была в + тогда мы получим наш прирост в цене
                        если сделка была в - тогда мы увидим на сколько в - мы ушли

                        -----*/
                    });
                }

                return candles;
            }

            catch (Exception ex)
            {
                return new List<Candle>();
            }
        }

        #endregion



        #region Socket

        //Сделки
        public event Action<Trade> NewBuyTrade; // Событие для покупок
        public event Action<Trade> NewSellTrade; // Событие для продаж

        public void SubscribeTrades(string pair, int maxCount = 100)
        {



            throw new NotImplementedException("Заглушка для подписки на сделки через WebSocket");
        }

        public void UnsubscribeTrades(string pair)
        {


            throw new NotImplementedException("Заглушка для отписки от сделок");
        }


        //Свечи
        public event Action<Candle> CandleSeriesProcessing; // Событие для обработки свечей

        public void SubscribeCandles(string pair, int periodInSec, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = 0)
        {



            throw new NotImplementedException("Заглушка для подписки на свечи через WebSocket");
        }

        public void UnsubscribeCandles(string pair)
        {



            throw new NotImplementedException("Заглушка для отписки от свечей");
        }

        #endregion
    }
}