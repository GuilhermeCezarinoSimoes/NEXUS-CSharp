using Microsoft.EntityFrameworkCore;
using TelemetryAPI.Data;
using TelemetryAPI.Interfaces;
using TelemetryAPI.Messaging;
using TelemetryAPI.Repositories;
using TelemetryAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Banco de Dados ───────────────────────────────────────────────────────────
// Em desenvolvimento usa InMemory (sem precisar de SQL Server instalado).
// Em produção (ASPNETCORE_ENVIRONMENT=Production) usa SQL Server real.
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<TelemetryDbContext>(options =>
        options.UseInMemoryDatabase("NEXUS_Telemetry_Dev"));
}
else
{
    builder.Services.AddDbContext<TelemetryDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// ── Repository Pattern (Injeção de Dependência) ──────────────────────────────
builder.Services.AddScoped<ITelemetryRepository, TelemetryRepository>();
builder.Services.AddScoped<IEnergyRepository, EnergyRepository>();

// ── RabbitMQ Publisher — falha silenciosa se broker indisponível ──────────────
builder.Services.AddSingleton<IMessagePublisher>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<RabbitMQPublisher>>();
    try
    {
        return RabbitMQPublisher.CreateAsync(config, logger).GetAwaiter().GetResult();
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "RabbitMQ indisponível — API iniciará sem mensageria. " +
            "Anomalias serão logadas mas não publicadas.");
        return new NullMessagePublisher(logger);
    }
});

// ── Service Layer ────────────────────────────────────────────────────────────
builder.Services.AddScoped<TelemetryService>();

// ── Swagger / OpenAPI ────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "NEXUS TelemetryAPI",
        Version = "v1",
        Description = "Microsserviço de Telemetria — coleta dados de saúde e energia da base NEXUS " +
                      "e publica alertas via RabbitMQ quando anomalias são detectadas."
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "NEXUS TelemetryAPI v1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
