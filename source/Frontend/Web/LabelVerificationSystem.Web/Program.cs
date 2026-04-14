using ApexCharts;
using BlazorColorPicker;
using LabelVerificationSystem.Web.Components;
using LabelVerificationSystem.Web.Components.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

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
            builder.Services.AddScoped<LabelVerificationSystem.Web.Components.ExcelUploads.ExcelUploadApiClient>();

            builder.Services.AddWMBOS();
            builder.Services.AddWMBSC();

            var configuredApiBaseUrl = builder.Configuration[BackendApiHttpClientOptions.BaseUrlConfigurationKey];
            var backendApiBaseUri = BuildBackendApiBaseUri(builder.HostEnvironment.BaseAddress, configuredApiBaseUrl);

            builder.Services.AddHttpClient(BackendApiHttpClientOptions.ClientName, client =>
            {
                client.BaseAddress = backendApiBaseUri;
            });

            await builder.Build().RunAsync();
        }

        private static Uri BuildBackendApiBaseUri(string hostBaseAddress, string? configuredApiBaseUrl)
        {
            if (string.IsNullOrWhiteSpace(configuredApiBaseUrl))
            {
                return EnsureTrailingSlash(new Uri(hostBaseAddress));
            }

            if (Uri.TryCreate(configuredApiBaseUrl, UriKind.Absolute, out var absoluteUri))
            {
                return EnsureTrailingSlash(absoluteUri);
            }

            var hostBaseUri = new Uri(hostBaseAddress);
            return EnsureTrailingSlash(new Uri(hostBaseUri, configuredApiBaseUrl));
        }

        private static Uri EnsureTrailingSlash(Uri baseUri)
        {
            var value = baseUri.ToString();
            return value.EndsWith('/') ? baseUri : new Uri($"{value}/");
        }
    }
}
