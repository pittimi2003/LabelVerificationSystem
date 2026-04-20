using System.Security.Claims;
using System.Text;
using LabelVerificationSystem.Api.Auth;
using LabelVerificationSystem.Infrastructure.Auth;
using LabelVerificationSystem.Infrastructure.DependencyInjection;
using LabelVerificationSystem.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        policy.RequireAssertion(context =>
            context.User.IsInRole("Administrator")
            || context.User.HasClaim(AuthPermissionClaims.Type, AuthPermissionClaims.UsersRead)
            || context.User.HasClaim(AuthPermissionClaims.Type, AuthPermissionClaims.UsersManage));
    });

    options.AddPolicy(AuthAuthorizationPolicies.UsersManage, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            context.User.IsInRole("Administrator")
            || context.User.HasClaim(AuthPermissionClaims.Type, AuthPermissionClaims.UsersManage));
    });
});

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
