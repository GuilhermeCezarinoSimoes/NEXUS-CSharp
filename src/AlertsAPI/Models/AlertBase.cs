namespace AlertsAPI.Models;

/// <summary>
/// Classe abstrata base para todos os tipos de alerta do NEXUS.
/// Define o contrato que qualquer alerta deve cumprir.
/// </summary>
public abstract class AlertBase
{
    public int Id { get; set; }
    public string AlertId { get; set; } = string.Empty;
    public string BaseLocation { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public EmergencyStatus Status { get; set; } = EmergencyStatus.Ativo;

    public abstract string GetDisplayTitle();
    public abstract bool RequiresImmediateAction();

    public bool IsRecent(int minutes = 30) =>
        (DateTime.UtcNow - ReceivedAt).TotalMinutes <= minutes;
}

public enum EmergencyStatus
{
    Ativo,
    EmAtendimento,
    Resolvido,
    Falso
}
