using BlazorColorPicker;
using LabelVerificationSystem.Web.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using ApexCharts;

namespace LabelVerificationSystem.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddColorPicker();
            builder.Services.AddMudServices();
            builder.Services.AddApexCharts();

            builder.Services.AddSingleton<AppState>();
            builder.Services.AddScoped<StateService>();
            builder.Services.AddScoped<IActionService, ActionService>();
            builder.Services.AddScoped<MenuDataService>();
            builder.Services.AddScoped<NavScrollService>();
            builder.Services.AddScoped<SessionService>();
            builder.Services.AddScoped<ScriptLoaderService>();

            builder.Services.AddWMBOS();
            builder.Services.AddWMBSC();

            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
            });

            await builder.Build().RunAsync();
        }
    }
}