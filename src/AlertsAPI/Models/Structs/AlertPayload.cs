namespace AlertsAPI.Models.Structs;

/// <summary>
/// Struct que representa o payload JSON deserializado da fila RabbitMQ.
/// Imutável por design — dados recebidos não devem ser alterados.
/// </summary>
public struct AlertPayload
{
    public string AlertId { get; init; }
    public string SensorType { get; init; }
    public string SensorId { get; init; }
    public string BaseLocation { get; init; }
    public string Description { get; init; }
    public DateTime OccurredAt { get; init; }
    public string Severity { get; init; }

    public readonly bool IsCritical() => Severity == "CRITICO";

    public readonly Alert ToAlert() => new()
    {
        AlertId = AlertId,
        SensorType = SensorType,
        SensorId = SensorId,
        BaseLocation = BaseLocation,
        Description = Description,
        Severity = Severity,
        ReceivedAt = DateTime.UtcNow,
        Status = AlertsAPI.Models.EmergencyStatus.Ativo
    };
}
