namespace TelemetryAPI.Models;

/// <summary>
/// Classe base abstrata para todas as leituras de sensores do NEXUS.
/// Garante que todo sensor implemente detecção de anomalia e descrição de alerta.
/// </summary>
public abstract class SensorBase
{
    public int Id { get; set; }
    public string SensorId { get; set; } = string.Empty;
    public string BaseLocation { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool Processed { get; set; } = false;

    public abstract bool IsAnomaly();
    public abstract string GetAlertDescription();
    public abstract string GetSensorType();

    public TimeSpan GetDataAge() => DateTime.UtcNow - Timestamp;

    public bool IsDataStale(int maxAgeMinutes = 5) =>
        GetDataAge().TotalMinutes > maxAgeMinutes;
}
