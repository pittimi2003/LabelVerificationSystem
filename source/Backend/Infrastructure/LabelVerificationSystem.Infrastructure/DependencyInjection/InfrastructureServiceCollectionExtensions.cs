using LabelVerificationSystem.Application.Interfaces.Auth;
using LabelVerificationSystem.Application.Interfaces.Authorization;
using LabelVerificationSystem.Application.Interfaces.ExcelUploads;
using LabelVerificationSystem.Application.Interfaces.Users;
using LabelVerificationSystem.Infrastructure.Auth;
using LabelVerificationSystem.Infrastructure.Authorization;
using LabelVerificationSystem.Infrastructure.ExcelUploads;
using LabelVerificationSystem.Infrastructure.Persistence;
using LabelVerificationSystem.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LabelVerificationSystem.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? "Data Source=label-verification.db";

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
        services.Configure<ExcelUploadStorageOptions>(configuration.GetSection(ExcelUploadStorageOptions.SectionName));
        services.Configure<AuthenticationOptions>(configuration.GetSection(AuthenticationOptions.SectionName));
        services.Configure<AuthorizationRuntimeOptions>(configuration.GetSection(AuthorizationRuntimeOptions.SectionName));
        services.AddScoped<IExcelUploadService, ExcelUploadService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAuthorizationMatrixService, AuthorizationMatrixService>();
        services.AddScoped<IAuthorizationAdministrationService, AuthorizationAdministrationService>();
        services.AddScoped<IUserAdministrationService, UserAdministrationService>();

        return services;
    }
}
