using System.Security.Claims;
using System.Text;
using LabelVerificationSystem.Api.Auth;
using LabelVerificationSystem.Api.Auth.Authorization;
using LabelVerificationSystem.Infrastructure.Auth;
using LabelVerificationSystem.Infrastructure.DependencyInjection;
using LabelVerificationSystem.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

const string LocalDevCorsPolicy = "LocalDevFrontend";
const string SmartAuthenticationScheme = "SmartAuthentication";

var localDevCorsOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? ["https://localhost:7219"];

builder.Services.AddCors(options =>
{
    options.AddPolicy(LocalDevCorsPolicy, policy =>
    {
        policy
            .WithOrigins(localDevCorsOrigins)
            .WithMethods("GET", "POST", "PUT", "PATCH")
            .AllowAnyHeader();
    });
});

builder.Services.AddInfrastructure(builder.Configuration);

var authOptions = builder.Configuration.GetSection(LabelVerificationSystem.Infrastructure.Auth.AuthenticationOptions.SectionName).Get<LabelVerificationSystem.Infrastructure.Auth.AuthenticationOptions>()
                  ?? new LabelVerificationSystem.Infrastructure.Auth.AuthenticationOptions();

if (string.IsNullOrWhiteSpace(authOptions.Jwt.SigningKey) || authOptions.Jwt.SigningKey.Length < 32)
{
    throw new InvalidOperationException("Authentication:Jwt:SigningKey es obligatorio y debe tener al menos 32 caracteres.");
}

builder.Services.AddSingleton(authOptions);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = SmartAuthenticationScheme;
        options.DefaultChallengeScheme = SmartAuthenticationScheme;
    })
    .AddPolicyScheme(SmartAuthenticationScheme, SmartAuthenticationScheme, options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            var authorization = context.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrWhiteSpace(authorization) && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return JwtBearerDefaults.AuthenticationScheme;
            }

            return authOptions.Bypass.Enabled ? BypassAuthenticationDefaults.Scheme : JwtBearerDefaults.AuthenticationScheme;
        };
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = authOptions.Jwt.Issuer,
            ValidAudience = authOptions.Jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.Jwt.SigningKey)),
            ClockSkew = TimeSpan.FromSeconds(authOptions.Jwt.ClockSkewSeconds)
        };
    })
    .AddScheme<AuthenticationSchemeOptions, BypassAuthenticationHandler>(BypassAuthenticationDefaults.Scheme, _ => { });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthAuthorizationPolicies.UsersRead, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new ModuleActionAuthorizationRequirement(AuthModules.UsersAdministration, AuthModuleActions.View));
    });

    options.AddPolicy(AuthAuthorizationPolicies.UsersCreate, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new ModuleActionAuthorizationRequirement(AuthModules.UsersAdministration, AuthModuleActions.Create));
    });

    options.AddPolicy(AuthAuthorizationPolicies.UsersEdit, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new ModuleActionAuthorizationRequirement(AuthModules.UsersAdministration, AuthModuleActions.Edit));
    });

    options.AddPolicy(AuthAuthorizationPolicies.UsersActivateDeactivate, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new ModuleActionAuthorizationRequirement(AuthModules.UsersAdministration, AuthModuleActions.ActivateDeactivate));
    });

    options.AddPolicy(AuthAuthorizationPolicies.AuthorizationMatrixManage, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new ModuleActionAuthorizationRequirement(AuthModules.AuthorizationMatrixAdministration, AuthModuleActions.Manage));
    });

    options.AddPolicy(AuthAuthorizationPolicies.ExcelUploadsRead, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new ModuleActionAuthorizationRequirement(AuthModules.ExcelUploads, AuthModuleActions.View));
    });

    options.AddPolicy(AuthAuthorizationPolicies.ExcelUploadsUpload, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new ModuleActionAuthorizationRequirement(AuthModules.ExcelUploads, AuthModuleActions.Upload));
    });


    options.AddPolicy(AuthAuthorizationPolicies.PartsRead, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new ModuleActionAuthorizationRequirement(AuthModules.PartsCatalog, AuthModuleActions.View));
    });

    options.AddPolicy(AuthAuthorizationPolicies.PartsCreate, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new ModuleActionAuthorizationRequirement(AuthModules.PartsCatalog, AuthModuleActions.Create));
    });

    options.AddPolicy(AuthAuthorizationPolicies.PartsEdit, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new ModuleActionAuthorizationRequirement(AuthModules.PartsCatalog, AuthModuleActions.Edit));
    });

    options.AddPolicy(AuthAuthorizationPolicies.RolesRead, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new ModuleActionAuthorizationRequirement(AuthModules.RolesCatalog, AuthModuleActions.View));
    });

    options.AddPolicy(AuthAuthorizationPolicies.RolesCreate, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new ModuleActionAuthorizationRequirement(AuthModules.RolesCatalog, AuthModuleActions.Create));
    });

    options.AddPolicy(AuthAuthorizationPolicies.RolesEdit, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new ModuleActionAuthorizationRequirement(AuthModules.RolesCatalog, AuthModuleActions.Edit));
    });

    options.AddPolicy(AuthAuthorizationPolicies.RolesActivateDeactivate, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new ModuleActionAuthorizationRequirement(AuthModules.RolesCatalog, AuthModuleActions.ActivateDeactivate));
    });


    options.AddPolicy(AuthAuthorizationPolicies.LabelTypesRead, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new ModuleActionAuthorizationRequirement(AuthModules.LabelTypes, AuthModuleActions.View));
    });

    options.AddPolicy(AuthAuthorizationPolicies.LabelTypesCreate, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new ModuleActionAuthorizationRequirement(AuthModules.LabelTypes, AuthModuleActions.Create));
    });

    options.AddPolicy(AuthAuthorizationPolicies.LabelTypesEdit, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new ModuleActionAuthorizationRequirement(AuthModules.LabelTypes, AuthModuleActions.Edit));
    });

    options.AddPolicy(AuthAuthorizationPolicies.LabelTypesActivateDeactivate, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new ModuleActionAuthorizationRequirement(AuthModules.LabelTypes, AuthModuleActions.ActivateDeactivate));
    });
});

builder.Services.AddScoped<IAuthorizationHandler, ModuleActionAuthorizationHandler>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors(LocalDevCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
