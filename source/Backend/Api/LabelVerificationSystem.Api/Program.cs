using Microsoft.EntityFrameworkCore;
using System.Text;
using LabelVerificationSystem.Infrastructure.Auth;
using LabelVerificationSystem.Infrastructure.DependencyInjection;
using LabelVerificationSystem.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

const string LocalDevCorsPolicy = "LocalDevFrontend";

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
            .WithMethods("GET", "POST")
            .AllowAnyHeader();
    });
});

builder.Services.AddInfrastructure(builder.Configuration);

var authOptions = builder.Configuration.GetSection(AuthenticationOptions.SectionName).Get<AuthenticationOptions>()
                  ?? new AuthenticationOptions();

if (string.IsNullOrWhiteSpace(authOptions.Jwt.SigningKey) || authOptions.Jwt.SigningKey.Length < 32)
{
    throw new InvalidOperationException("Authentication:Jwt:SigningKey es obligatorio y debe tener al menos 32 caracteres.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
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
    });

builder.Services.AddAuthorization();
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
