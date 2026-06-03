using AlertsAPI.Data;
using AlertsAPI.Interfaces;
using AlertsAPI.Messaging;
using AlertsAPI.Repositories;
using AlertsAPI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Banco de Dados ───────────────────────────────────────────────────────────
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<AlertsDbContext>(options =>
        options.UseInMemoryDatabase("NEXUS_Alerts_Dev"));
}
else
{
    builder.Services.AddDbContext<AlertsDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// ── Repository Pattern ────────────────────────────────────────────────────────
builder.Services.AddScoped<IAlertRepository, AlertRepository>();

// ── Service Layer ────────────────────────────────────────────────────────────
builder.Services.AddScoped<AlertService>();

// ── RabbitMQ Consumer (BackgroundService) ─────────────────────────────────────
builder.Services.AddHostedService<RabbitMQConsumer>();

// ── Swagger / OpenAPI ────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "NEXUS AlertsAPI",
        Version = "v1",
        Description = "Microsserviço de Alertas — consome eventos de anomalia do RabbitMQ (publicados " +
                      "pela TelemetryAPI), registra ocorrências e gerencia protocolos de emergência da base NEXUS."
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "NEXUS AlertsAPI v1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
