using TelemetryAPI.Models;

namespace TelemetryAPI.Services;

/// <summary>
/// Parte da TelemetryService responsável pela validação de entrada dos dados de sensores.
/// Separada em partial class para manter o arquivo principal focado na orquestração.
/// </summary>
public partial class TelemetryService
{
    private static void ValidateHealthReading(TelemetryReading reading)
    {
        ArgumentNullException.ThrowIfNull(reading);

        if (string.IsNullOrWhiteSpace(reading.SensorId))
            throw new ArgumentException("SensorId é obrigatório.", nameof(reading));

        if (string.IsNullOrWhiteSpace(reading.OperatorId))
            throw new ArgumentException("OperatorId é obrigatório.", nameof(reading));

        if (reading.Temperature is < -273.15 or > 1000)
            throw new ArgumentOutOfRangeException(nameof(reading),
                $"Temperatura fisicamente impossível: {reading.Temperature}°C");

        if (reading.HeartRate is < 0 or > 300)
            throw new ArgumentOutOfRangeException(nameof(reading),
                $"Batimento cardíaco inválido: {reading.HeartRate} bpm");

        if (reading.OxygenLevel is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(reading),
                $"Nível de oxigênio inválido: {reading.OxygenLevel}%");
    }

    private static void ValidateEnergyReading(EnergyReading reading)
    {
        ArgumentNullException.ThrowIfNull(reading);

        if (string.IsNullOrWhiteSpace(reading.SensorId))
            throw new ArgumentException("SensorId é obrigatório.", nameof(reading));

        if (reading.EnergyLevel is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(reading),
                $"Nível de energia inválido: {reading.EnergyLevel}%");

        if (reading.VoltageV < 0)
            throw new ArgumentOutOfRangeException(nameof(reading),
                $"Voltagem não pode ser negativa: {reading.VoltageV}V");

        if (reading.PowerConsumptionKw < 0)
            throw new ArgumentOutOfRangeException(nameof(reading),
                $"Consumo de energia não pode ser negativo: {reading.PowerConsumptionKw}kW");
    }
}
