using System.ComponentModel.DataAnnotations;

namespace AlertsAPI.Models;

/// <summary>
/// Representa um alerta registrado no banco de dados da AlertsAPI.
/// Herda de AlertBase (classe base abstrata) para garantir contrato uniforme de alertas.
/// </summary>
public class Alert : AlertBase
{
    [Required]
    [MaxLength(50)]
    public string SensorType { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string SensorId { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Severity { get; set; } = "ALTO";

    public bool Acknowledged { get; set; } = false;
    public DateTime? AcknowledgedAt { get; set; }
    public string? AcknowledgedBy { get; set; }

    public override string GetDisplayTitle() =>
        $"[{Severity}] {SensorType} @ {BaseLocation}";

    public override bool RequiresImmediateAction() =>
        Severity == "CRITICO" && !Acknowledged;

    public void Acknowledge(string operatorId)
    {
        if (Acknowledged)
            throw new InvalidOperationException($"Alerta {Id} já foi confirmado por {AcknowledgedBy}.");

        Acknowledged = true;
        AcknowledgedAt = DateTime.UtcNow;
        AcknowledgedBy = operatorId;
    }

    public TimeSpan GetResponseTime()
    {
        if (!Acknowledged || AcknowledgedAt == null)
            return DateTime.UtcNow - ReceivedAt;
        return AcknowledgedAt.Value - ReceivedAt;
    }
}
