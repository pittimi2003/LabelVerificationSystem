using ApexCharts;
using BlazorColorPicker;
using LabelVerificationSystem.Web.Components;
using LabelVerificationSystem.Web.Components.Auth;
using LabelVerificationSystem.Web.Components.Authorization;
using LabelVerificationSystem.Web.Components.Roles;
using LabelVerificationSystem.Web.Components.Services;
using LabelVerificationSystem.Web.Components.Users;
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
            builder.Services.AddScoped<AuthSessionStorage>();
            builder.Services.AddScoped<AuthApiClient>();
            builder.Services.AddScoped<AuthSessionService>();
            builder.Services.AddScoped<BackendApiAuthHandler>();
            builder.Services.AddWMBOS();
            builder.Services.AddWMBSC();

            var configuredApiBaseUrl = builder.Configuration[BackendApiHttpClientOptions.BaseUrlConfigurationKey];
            var backendApiBaseUri = BuildBackendApiBaseUri(configuredApiBaseUrl);

            builder.Services.AddHttpClient(BackendApiHttpClientOptions.RawClientName, client =>
            {
                client.BaseAddress = backendApiBaseUri;
            });

            builder.Services.AddHttpClient(BackendApiHttpClientOptions.ClientName, client =>
            {
                client.BaseAddress = backendApiBaseUri;
            }).AddHttpMessageHandler<BackendApiAuthHandler>();
            builder.Services.AddScoped(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var backendApiClient = httpClientFactory.CreateClient(BackendApiHttpClientOptions.RawClientName);
                var authSessionService = sp.GetRequiredService<AuthSessionService>();
                return new LabelVerificationSystem.Web.Components.ExcelUploads.ExcelUploadApiClient(backendApiClient, authSessionService);
            });
            builder.Services.AddScoped(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var backendApiClient = httpClientFactory.CreateClient(BackendApiHttpClientOptions.RawClientName);
                var authSessionService = sp.GetRequiredService<AuthSessionService>();
                return new UserAdministrationApiClient(backendApiClient, authSessionService);
            });
            builder.Services.AddScoped(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var backendApiClient = httpClientFactory.CreateClient(BackendApiHttpClientOptions.RawClientName);
                var authSessionService = sp.GetRequiredService<AuthSessionService>();
                return new AuthorizationAdministrationApiClient(backendApiClient, authSessionService);
            });
            builder.Services.AddScoped(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var backendApiClient = httpClientFactory.CreateClient(BackendApiHttpClientOptions.RawClientName);
                var authSessionService = sp.GetRequiredService<AuthSessionService>();
                return new RoleCatalogAdministrationApiClient(backendApiClient, authSessionService);
            });

            await builder.Build().RunAsync();
        }

        private static Uri BuildBackendApiBaseUri(string? configuredApiBaseUrl)
        {
            if (string.IsNullOrWhiteSpace(configuredApiBaseUrl))
            {
                throw new InvalidOperationException(
                    $"Missing required configuration '{BackendApiHttpClientOptions.BaseUrlConfigurationKey}'. Set it to an absolute API URL, for example 'https://localhost:7131/'.");
            }

            if (!Uri.TryCreate(configuredApiBaseUrl, UriKind.Absolute, out var absoluteUri))
            {
                throw new InvalidOperationException(
                    $"Configuration '{BackendApiHttpClientOptions.BaseUrlConfigurationKey}' must be an absolute URL. Current value: '{configuredApiBaseUrl}'.");
            }

            return EnsureTrailingSlash(absoluteUri);
        }

        private static Uri EnsureTrailingSlash(Uri baseUri)
        {
            var value = baseUri.ToString();
            return value.EndsWith('/') ? baseUri : new Uri($"{value}/");
        }
    }
}
