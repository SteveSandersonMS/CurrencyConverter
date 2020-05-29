namespace CurrencyConverter.Data
{
    public class ExchangeRate
    {
        public ExchangeRate(Currency currency, decimal valueInEur)
        {
            Currency = currency;
            ValueInEUR = valueInEur;
        }

        public Currency Currency { get; }
        public decimal ValueInEUR { get; }
    }
}
