using System.ComponentModel.DataAnnotations;

namespace TelemetryAPI.Models;

/// <summary>
/// Leitura dos sensores de energia: nível de bateria, tensão e status do gerador.
/// Usada pelo Módulo de Energia (Energy Module) do NEXUS.
/// </summary>
public class EnergyReading : SensorBase
{
    [Required]
    [Range(0.0, 100.0, ErrorMessage = "Nível de energia deve estar entre 0% e 100%.")]
    public double EnergyLevel { get; set; }

    [Required]
    [Range(0.0, 500.0)]
    public double VoltageV { get; set; }

    public bool GeneratorOnline { get; set; } = true;
    public double PowerConsumptionKw { get; set; }

    private const double EnergyMinCritical = 15.0;
    private const double EnergyMinWarning = 30.0;
    private const double VoltageMin = 110.0;
    private const double VoltageMax = 240.0;

    public override bool IsAnomaly() =>
        EnergyLevel < EnergyMinCritical ||
        !GeneratorOnline ||
        VoltageV < VoltageMin ||
        VoltageV > VoltageMax;

    public override string GetAlertDescription()
    {
        var issues = new List<string>();

        if (EnergyLevel < EnergyMinCritical)
            issues.Add($"Nível de energia crítico: {EnergyLevel:F1}% (mínimo: {EnergyMinCritical}%)");
        else if (EnergyLevel < EnergyMinWarning)
            issues.Add($"Aviso de energia baixa: {EnergyLevel:F1}%");
        if (!GeneratorOnline)
            issues.Add("GERADOR OFFLINE — Sistema operando em bateria de emergência!");
        if (VoltageV < VoltageMin)
            issues.Add($"Sub-tensão: {VoltageV:F1}V (mínimo: {VoltageMin}V)");
        if (VoltageV > VoltageMax)
            issues.Add($"Sobre-tensão: {VoltageV:F1}V (máximo: {VoltageMax}V)");

        return issues.Count > 0
            ? "[ENERGIA] " + string.Join(" | ", issues)
            : "Sistema de energia operando normalmente.";
    }

    public override string GetSensorType() => "ENERGY";

    public static EnergyReading CreateEmergencyReading(string sensorId, string location) =>
        new()
        {
            SensorId = sensorId,
            BaseLocation = location,
            EnergyLevel = 5.0,
            VoltageV = 0.0,
            GeneratorOnline = false,
            PowerConsumptionKw = 0,
            Timestamp = DateTime.UtcNow
        };
}
