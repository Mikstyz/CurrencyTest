namespace Service.Entites.Bybut
{
    public class EInCurrencyConvert // Конвертор валюты
    {
        public string Symbol { get; set; }
        public decimal Price { get; set; } //цена валюты (кол-во)
        public string Symbol_tsyms { get; set; }
    }

    public class EInCurrencyConvertGroup // конвертор пары вылют
    {
        public Dictionary<string, decimal> Symbol_fsymMap { get; set; }
        public string Symbol_tsyms { get; set; }
    }
}