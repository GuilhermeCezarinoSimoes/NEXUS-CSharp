namespace TelemetryAPI.Models.Structs;

/// <summary>
/// Struct leve que representa a mensagem publicada no RabbitMQ ao detectar uma anomalia.
/// Structs são adequadas aqui por serem dados imutáveis e de curta duração.
/// </summary>
public struct AlertMessage
{
    public string AlertId { get; init; }
    public string SensorType { get; init; }
    public string SensorId { get; init; }
    public string BaseLocation { get; init; }
    public string Description { get; init; }
    public DateTime OccurredAt { get; init; }
    public string Severity { get; init; }

    public AlertMessage(string sensorType, string sensorId, string location,
                        string description, string severity)
    {
        AlertId = Guid.NewGuid().ToString();
        SensorType = sensorType;
        SensorId = sensorId;
        BaseLocation = location;
        Description = description;
        OccurredAt = DateTime.UtcNow;
        Severity = severity;
    }

    public override readonly string ToString() =>
        $"[{Severity}] {SensorType}@{BaseLocation} | {Description} | {OccurredAt:yyyy-MM-dd HH:mm:ss}Z";
}
