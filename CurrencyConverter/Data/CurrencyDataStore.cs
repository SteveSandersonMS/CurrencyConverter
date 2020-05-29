using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace CurrencyConverter.Data
{
    public class CurrencyDataStore
    {
        private readonly HttpClient httpClient;
        private readonly IJSInProcessRuntime js;
        private readonly string fxDataFeedUrl;

        private readonly List<Currency> supportedCurrencies = new List<Currency>
        {
            new Currency { Code = "EUR", Flag = "EU.png", Sign = "€" },
            new Currency { Code = "USD", Flag = "US.png", Sign = "$" },
            new Currency { Code = "GBP", Flag = "GB.png", Sign = "£" },
            new Currency { Code = "CNY", Flag = "CN.png", Sign = "¥" },
            new Currency { Code = "JPY", Flag = "JP.png", Sign = "¥" },
            new Currency { Code = "CAD", Flag = "CA.png", Sign = "$" },
            new Currency { Code = "BRL", Flag = "BR.png", Sign = "R$" },
            new Currency { Code = "SEK", Flag = "SE.png", Sign = "kr" },
            new Currency { Code = "INR", Flag = "IN.png", Sign = "₹" },
        };

        public CurrencyDataStore(HttpClient httpClient, IJSRuntime js, IConfiguration config)
        {
            this.httpClient = httpClient;
            this.js = (IJSInProcessRuntime)js;
            fxDataFeedUrl = config.GetSection("Services")["FxDataFeed"];
            Populate();

            if (IsStale || !LastUpdatedDate.HasValue)
            {
                // In the background, also attempt to refresh the data
                // If the user doesn't have a working network connection, this will fail, and that's fine
                _ = Refresh();
            }
        }

        public DateTime? LastUpdatedDate { get; private set; }
        public List<ExchangeRate> ExchangeRates { get; } = new List<ExchangeRate>();
        public Dictionary<string, ExchangeRate> ExchangeRatesLookup { get; } = new Dictionary<string, ExchangeRate>();
        public bool IsLoading { get; set; }
        public bool IsStale => !IsLoading
            && LastUpdatedDate.HasValue
            && DateTime.Now.Subtract(LastUpdatedDate.Value).TotalDays > 2;

        public delegate void LoadingStatusChangedHandler();
        public LoadingStatusChangedHandler LoadingStatusChanged;

        public async Task Refresh()
        {
            UpdateLoadingState(true);

            try
            {
                var response = await httpClient.GetFromJsonAsync<RawFxData>(fxDataFeedUrl);
                response.Rates.Add("EUR", 1);
                await js.InvokeVoidAsync("localStorage.setItem", "fxData", JsonSerializer.Serialize(response));
            }
            finally
            {
                Populate();
                UpdateLoadingState(false);
            }
        }

        void UpdateLoadingState(bool isLoading)
        {
            IsLoading = isLoading;
            LoadingStatusChanged?.Invoke();
        }

        void Populate()
        {
            var json = js.Invoke<string>("localStorage.getItem", "fxData");
            if (json != null)
            {
                var rawData = JsonSerializer.Deserialize<RawFxData>(json);
                LastUpdatedDate = rawData.Date;

                ExchangeRates.Clear();
                ExchangeRatesLookup.Clear();
                foreach (var currency in supportedCurrencies)
                {
                    if (rawData.Rates.TryGetValue(currency.Code, out var valueInEur))
                    {
                        var exchangeRate = new ExchangeRate(currency, valueInEur);
                        ExchangeRates.Add(exchangeRate);
                        ExchangeRatesLookup.Add(currency.Code, exchangeRate);
                    }
                }
            }
        }

        class RawFxData
        {
            public DateTime Date { get; set; }
            public Dictionary<string, decimal> Rates { get; set; }
        }
    }
}
