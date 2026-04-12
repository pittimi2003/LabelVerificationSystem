using MudBlazor.Services;
using LabelVerificationSystem.Web.Components;
using BlazorColorPicker;
using ApexCharts;

namespace LabelVerificationSystem.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddCircuitOptions(options =>
                {
                    options.DetailedErrors = true;
                });

            builder.Services.AddColorPicker();
            builder.Services.AddMudServices();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSingleton<AppState>();
            builder.Services.AddScoped<StateService>();
            builder.Services.AddScoped<IActionService, ActionService>();
            builder.Services.AddWMBOS();
            builder.Services.AddWMBSC();
            builder.Services.AddScoped<MenuDataService>();
            builder.Services.AddScoped<NavScrollService>();
            builder.Services.AddScoped<SessionService>();
            builder.Services.AddScoped<ScriptLoaderService>();

            // 👇 Required for Session
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Register HttpClient service
            builder.Services.AddHttpClient(); 

            // 👇 If you want MVC controllers/views too
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error", createScopeForErrors: true);
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // 👇 Enable session BEFORE antiforgery/static files/endpoints
            app.UseSession();
            app.UseAntiforgery();

            app.UseStaticFiles();
            app.MapStaticAssets();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            // If using MVC controllers
            app.MapControllers();

            app.Run();
        }
    }
}
