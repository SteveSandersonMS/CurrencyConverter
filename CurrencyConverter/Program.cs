using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using CurrencyConverter.Data;
using CurrencyConverter.UI;

namespace CurrencyConverter
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddScoped<CurrencyDataStore>();
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

            await builder.Build().RunAsync();
        }
    }
}
