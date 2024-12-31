using HttpObservable.Samples.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace HttpObservable_Samples
{
    public static class ApiEnd
    {
        public static string BaseUrl = "https://ap6d8web.we-qdc.wcp.wuerth.com:9000/";
    }
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(ApiEnd.BaseUrl) });
          
          
            builder.Services.AddScoped<SampleService>();

            await builder.Build().RunAsync();
        }
    }
}
