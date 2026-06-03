using System.ComponentModel.DataAnnotations;

namespace AlertsAPI.Models;

/// <summary>
/// Incidente de emergência gerado quando vários alertas críticos convergem.
/// Herda de AlertBase e representa escalada máxima do protocolo NEXUS.
/// </summary>
public class EmergencyIncident : AlertBase
{
    [Required]
    [MaxLength(100)]
    public string IncidentTitle { get; set; } = string.Empty;

    public string AffectedSystems { get; set; } = string.Empty;
    public int AlertCount { get; set; }
    public bool IsolationProtocolTriggered { get; set; } = false;
    public DateTime? IsolationTriggeredAt { get; set; }
    public string? ResponsibleOperator { get; set; }

    public override string GetDisplayTitle() =>
        $"INCIDENTE: {IncidentTitle} | {AlertCount} alertas | " +
        $"Isolamento: {(IsolationProtocolTriggered ? "ATIVADO" : "PENDENTE")}";

    public override bool RequiresImmediateAction() =>
        Status == EmergencyStatus.Ativo && !IsolationProtocolTriggered;

    public void TriggerIsolationProtocol(string operatorId)
    {
        IsolationProtocolTriggered = true;
        IsolationTriggeredAt = DateTime.UtcNow;
        ResponsibleOperator = operatorId;
        Status = EmergencyStatus.EmAtendimento;
    }

    public static EmergencyIncident CreateFromAlerts(IEnumerable<Alert> alerts, string location)
    {
        var alertList = alerts.ToList();
        return new EmergencyIncident
        {
            AlertId = Guid.NewGuid().ToString(),
            IncidentTitle = $"Emergência Múltipla em {location}",
            BaseLocation = location,
            AlertCount = alertList.Count,
            AffectedSystems = string.Join(", ", alertList.Select(a => a.SensorType).Distinct()),
            Status = EmergencyStatus.Ativo,
            ReceivedAt = DateTime.UtcNow
        };
    }
}
