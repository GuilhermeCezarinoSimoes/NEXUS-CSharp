using System.ComponentModel.DataAnnotations;

namespace TelemetryAPI.Models;

/// <summary>
/// Leitura de telemetria combinada: temperatura ambiente + dados biométricos do operador.
/// Usada pelo Módulo de Saúde (Health Module) do NEXUS.
/// </summary>
public class TelemetryReading : SensorBase
{
    [Required]
    [Range(-100.0, 200.0, ErrorMessage = "Temperatura fora do intervalo operacional.")]
    public double Temperature { get; set; }

    [Required]
    [Range(0, 300, ErrorMessage = "Batimento cardíaco inválido.")]
    public int HeartRate { get; set; }

    [Required]
    public string OperatorId { get; set; } = string.Empty;

    public double OxygenLevel { get; set; } = 100.0;

    private const double TempMinCritical = -40.0;
    private const double TempMaxCritical = 60.0;
    private const int HeartRateMin = 40;
    private const int HeartRateMax = 180;
    private const double OxygenMin = 90.0;

    public override bool IsAnomaly() =>
        Temperature < TempMinCritical ||
        Temperature > TempMaxCritical ||
        HeartRate < HeartRateMin ||
        HeartRate > HeartRateMax ||
        OxygenLevel < OxygenMin;

    public override string GetAlertDescription()
    {
        var issues = new List<string>();

        if (Temperature < TempMinCritical)
            issues.Add($"Temperatura crítica baixa: {Temperature:F1}°C (limite: {TempMinCritical}°C)");
        if (Temperature > TempMaxCritical)
            issues.Add($"Temperatura crítica alta: {Temperature:F1}°C (limite: {TempMaxCritical}°C)");
        if (HeartRate < HeartRateMin)
            issues.Add($"Bradicardia detectada: {HeartRate} bpm (mínimo: {HeartRateMin} bpm)");
        if (HeartRate > HeartRateMax)
            issues.Add($"Taquicardia detectada: {HeartRate} bpm (máximo: {HeartRateMax} bpm)");
        if (OxygenLevel < OxygenMin)
            issues.Add($"Hipóxia detectada: SpO2 {OxygenLevel:F1}% (mínimo: {OxygenMin}%)");

        return issues.Count > 0
            ? $"[SAÚDE] Operador {OperatorId}: " + string.Join(" | ", issues)
            : "Leitura dentro dos parâmetros normais.";
    }

    public override string GetSensorType() => "HEALTH";

    public SeverityLevel GetSeverity()
    {
        if (HeartRate < 30 || HeartRate > 200 || OxygenLevel < 85.0)
            return SeverityLevel.Critico;
        if (IsAnomaly())
            return SeverityLevel.Alto;
        return SeverityLevel.Normal;
    }
}

public enum SeverityLevel
{
    Normal,
    Alto,
    Critico
}
