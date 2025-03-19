
using Serilog;
using Service.Entites.Bybut;
using System.Text.Json;

namespace Server.Bybit
{
    public class BybitApi
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<decimal?> CurrencyConversionAsync(EInCurrencyConvert curr)
        {
            if (curr == null || string.IsNullOrWhiteSpace(curr.Symbol) || string.IsNullOrWhiteSpace(curr.Symbol_tsyms) || curr.Price <= 0)
            {
                Log.Warning("Данные для конвертации отсутствуют");
                throw new ArgumentException("Некорректные данные для конверсии валюты");
            }

            if (curr.Symbol.Equals(curr.Symbol_tsyms, StringComparison.OrdinalIgnoreCase))
            {
                Log.Information("Валюты совпадают");
                return curr.Price;
            }

            try
            {
                string url = $"https://min-api.cryptocompare.com/data/price?fsym={curr.Symbol}&tsyms={curr.Symbol_tsyms}&api_key=93d5ae3d96a2ff9da78560537b356a9cc9c84ff7441ae51e77223ea3ac121e78";

                Log.Information($"Конвертация: {curr.Symbol} в {curr.Symbol_tsyms}");
                HttpResponseMessage response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Log.Warning($"Ошибка запроса к CryptoCompare: StatusCode={response.StatusCode}");
                    return null;
                }

                string json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<JsonElement>(json);

                if (data.TryGetProperty(curr.Symbol_tsyms, out JsonElement price))
                {
                    decimal result = curr.Price * price.GetDecimal();
                    Log.Information($"Конвертация\nPrice: {curr.Price} Currency: {curr.Symbol}\nPrice:{result} Currency: {curr.Symbol_tsyms}");
                    return result;
                }

                Log.Warning("Цена для {cur.tsyms} не найдена в ответе: {json}");
                return null;
            }

            catch (Exception ex)
            {
                Log.Error($"Ошибка при конвертации валюты: {curr.Symbol} в {curr.Symbol_tsyms}\n{ex}");
                throw new ArgumentException("Ошибка при конвертации валюты", ex);
            }
        }

        public static async Task<Dictionary<string, decimal>> CurrencyConversionGroupAsync(EInCurrencyConvertGroup curr)
        {
            string CurrIn = string.Join(",", curr.Symbol_fsymMap.Keys);
            string CurrOut = string.Join(",", curr.Symbol_tsyms);

            try
            {
                string url = $"https://min-api.cryptocompare.com/data/pricemulti?fsyms={CurrIn}&tsyms={CurrOut}&relaxedValidation=true&sign=true&tryConversion=true&api_key=93d5ae3d96a2ff9da78560537b356a9cc9c84ff7441ae51e77223ea3ac121e78";

                using HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Log.Warning($"Ошибка запроса: {response.StatusCode}");
                    return null;
                }

                string json = await response.Content.ReadAsStringAsync();
                var currencyData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, decimal>>>(json);

                if (currencyData == null || currencyData.Count == 0)
                {
                    Log.Warning("Api пуст");
                    return null;
                }

                var convertedBalances = new Dictionary<string, decimal>();

                foreach (var currency in currencyData)
                {
                    if (!curr.Symbol_fsymMap.TryGetValue(currency.Key, out decimal amount))
                    {
                        Log.Debug($"Валюта {currency.Key} не найдена в Symbol_fsymMap");
                        continue;
                    }

                    foreach (var rate in currency.Value)
                    {
                        if (!convertedBalances.ContainsKey(rate.Key))
                        {
                            convertedBalances[rate.Key] = 0;
                        }

                        decimal convertedAmount = amount * rate.Value;
                        convertedBalances[rate.Key] += convertedAmount;
                    }
                }

                return convertedBalances;
            }

            catch (Exception ex)
            {
                Log.Error($"Ошибка при конвертации списка валют: {CurrIn} в {CurrOut}\n{ex}");
                throw new ArgumentException("Ошибка при конвертации валюты", ex);
            }
        }

    }
}