using AlertsAPI.Interfaces;
using AlertsAPI.Models;
using AlertsAPI.Models.Structs;

namespace AlertsAPI.Services;

public class AlertService
{
    private readonly IAlertRepository _repo;
    private readonly ILogger<AlertService> _logger;

    private const int CriticalThresholdForIncident = 3;

    public AlertService(IAlertRepository repo, ILogger<AlertService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<Alert> RegisterAlertAsync(AlertPayload payload)
    {
        var existing = await _repo.GetByAlertIdAsync(payload.AlertId);
        if (existing != null)
        {
            _logger.LogInformation("Alerta duplicado ignorado: {AlertId}", payload.AlertId);
            return existing;
        }

        var alert = payload.ToAlert();
        var saved = await _repo.AddAsync(alert);

        _logger.LogWarning("Alerta registrado: [{Severity}] {SensorType}@{Location} — {Desc}",
            saved.Severity, saved.SensorType, saved.BaseLocation, saved.Description);

        if (payload.IsCritical())
            await CheckAndTriggerEmergencyProtocolAsync(saved.BaseLocation);

        return saved;
    }

    public async Task<Alert> AcknowledgeAlertAsync(int id, string operatorId)
    {
        var alert = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Alerta {id} não encontrado.");

        alert.Acknowledge(operatorId);
        alert.Status = EmergencyStatus.EmAtendimento;
        await _repo.UpdateAsync(alert);

        _logger.LogInformation("Alerta {Id} confirmado por {Op} em {Time}",
            id, operatorId, alert.AcknowledgedAt);
        return alert;
    }

    public async Task<Alert> ResolveAlertAsync(int id)
    {
        var alert = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Alerta {id} não encontrado.");

        alert.Status = EmergencyStatus.Resolvido;
        await _repo.UpdateAsync(alert);
        return alert;
    }

    public async Task<object> GetStatusSummaryAsync()
    {
        var active = await _repo.GetActiveAlertsAsync();
        var activeList = active.ToList();
        var criticalCount = activeList.Count(a => a.Severity == "CRITICO");
        var oldestActive = activeList.LastOrDefault();

        return new
        {
            TotalAtivos = activeList.Count,
            Criticos = criticalCount,
            Altos = activeList.Count(a => a.Severity == "ALTO"),
            OldestAlertAge = oldestActive != null
                ? $"{(DateTime.UtcNow - oldestActive.ReceivedAt).TotalMinutes:F0} minutos"
                : "N/A",
            SystemStatus = criticalCount > 0 ? "EMERGENCIA" :
                           activeList.Count > 0 ? "ATENCAO" : "NOMINAL",
            GeneratedAt = DateTime.UtcNow
        };
    }

    private async Task CheckAndTriggerEmergencyProtocolAsync(string location)
    {
        var recentCritical = (await _repo.GetByLocationAsync(location))
            .Where(a => a.Severity == "CRITICO" && a.IsRecent(15))
            .ToList();

        if (recentCritical.Count >= CriticalThresholdForIncident)
        {
            _logger.LogCritical(
                "PROTOCOLO DE EMERGÊNCIA: {Count} alertas críticos em {Location} nos últimos 15 min!",
                recentCritical.Count, location);
        }
    }

    public async Task<IEnumerable<Alert>> GetByLocationAsync(string location) =>
        await _repo.GetByLocationAsync(location);

    public async Task<IEnumerable<Alert>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        if (from > to)
            throw new ArgumentException("Data 'from' não pode ser posterior a 'to'.");
        return await _repo.GetByDateRangeAsync(from, to);
    }
}
