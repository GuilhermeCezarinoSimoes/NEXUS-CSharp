using TelemetryAPI.Interfaces;
using TelemetryAPI.Models;
using TelemetryAPI.Models.Structs;

namespace TelemetryAPI.Services;

/// <summary>
/// Orquestra o processamento de leituras de telemetria:
/// persiste no banco via Repository e publica alertas no RabbitMQ se anomalia detectada.
/// Classe parcial: regras de validação ficam em TelemetryService.Validation.cs.
/// </summary>
public partial class TelemetryService
{
    private readonly ITelemetryRepository _telemetryRepo;
    private readonly IEnergyRepository _energyRepo;
    private readonly IMessagePublisher _publisher;
    private readonly ILogger<TelemetryService> _logger;

    public TelemetryService(
        ITelemetryRepository telemetryRepo,
        IEnergyRepository energyRepo,
        IMessagePublisher publisher,
        ILogger<TelemetryService> logger)
    {
        _telemetryRepo = telemetryRepo;
        _energyRepo = energyRepo;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<TelemetryReading> ProcessHealthReadingAsync(TelemetryReading reading)
    {
        ValidateHealthReading(reading);

        var saved = await _telemetryRepo.AddAsync(reading);
        _logger.LogInformation("Leitura de saúde salva: Operador={Op}, BPM={HR}, Temp={T}°C",
            reading.OperatorId, reading.HeartRate, reading.Temperature);

        if (reading.IsAnomaly())
        {
            var severity = reading.GetSeverity() == SeverityLevel.Critico ? "CRITICO" : "ALTO";
            var alert = new AlertMessage(
                reading.GetSensorType(),
                reading.SensorId,
                reading.BaseLocation,
                reading.GetAlertDescription(),
                severity);

            await _publisher.PublishAlertAsync(alert);
            _logger.LogWarning("Anomalia detectada e alerta publicado: {Alert}", alert.ToString());

            saved.Processed = true;
            await _telemetryRepo.UpdateAsync(saved);
        }

        return saved;
    }

    public async Task<EnergyReading> ProcessEnergyReadingAsync(EnergyReading reading)
    {
        ValidateEnergyReading(reading);

        var saved = await _energyRepo.AddAsync(reading);
        _logger.LogInformation("Leitura de energia salva: Local={Loc}, Nivel={E}%, Gerador={G}",
            reading.BaseLocation, reading.EnergyLevel, reading.GeneratorOnline ? "ONLINE" : "OFFLINE");

        if (reading.IsAnomaly())
        {
            var alert = new AlertMessage(
                reading.GetSensorType(),
                reading.SensorId,
                reading.BaseLocation,
                reading.GetAlertDescription(),
                reading.GeneratorOnline ? "ALTO" : "CRITICO");

            await _publisher.PublishAlertAsync(alert);
            _logger.LogWarning("Anomalia de energia detectada: {Alert}", alert.ToString());
        }

        return saved;
    }

    public async Task<IEnumerable<TelemetryReading>> GetHistoryByOperatorAsync(
        string operatorId, DateTime? from, DateTime? to)
    {
        if (from.HasValue && to.HasValue && from > to)
            throw new ArgumentException("Data 'from' não pode ser posterior a 'to'.");

        if (from.HasValue && to.HasValue)
            return await _telemetryRepo.GetByDateRangeAsync(from.Value, to.Value);

        return await _telemetryRepo.GetByOperatorAsync(operatorId);
    }

    public async Task<object> GetDashboardSummaryAsync()
    {
        var readings = (await _telemetryRepo.GetAllAsync()).Take(100).ToList();
        var anomalies = readings.Where(r => r.IsAnomaly()).ToList();
        var lastReading = readings.FirstOrDefault();

        return new
        {
            TotalReadings = readings.Count,
            AnomaliesLast100 = anomalies.Count,
            LastReadingAt = lastReading?.Timestamp,
            LastReadingAge = lastReading != null
                ? $"{(DateTime.UtcNow - lastReading.Timestamp).TotalSeconds:F0}s atrás"
                : "N/A",
            SystemStatus = anomalies.Count == 0 ? "NOMINAL" :
                           anomalies.Count < 3 ? "ATENCAO" : "EMERGENCIA"
        };
    }
}
