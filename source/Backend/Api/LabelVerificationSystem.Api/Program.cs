using LabelVerificationSystem.Infrastructure.DependencyInjection;
using LabelVerificationSystem.Infrastructure.Persistence;

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
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Provisional para v1: EnsureCreated habilita persistencia mínima local mientras no exista estrategia formal de migraciones.
    dbContext.Database.EnsureCreated();
}


app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors(LocalDevCorsPolicy);
app.UseAuthorization();
app.MapControllers();

app.Run();
