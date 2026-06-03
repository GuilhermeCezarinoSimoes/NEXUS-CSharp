using TelemetryAPI.Models;

namespace NEXUS.Tests;

/// <summary>
/// Testes das regras de negócio implementadas diretamente nos modelos (IsAnomaly, GetSeverity).
/// Estes testes não precisam de mocks — testam lógica pura.
/// </summary>
public class SensorModelTests
{
    [Theory]
    [InlineData(-50.0, 70, 98.0, true)]   // temperatura abaixo do limite
    [InlineData(70.0,  70, 98.0, true)]   // temperatura acima do limite
    [InlineData(25.0,  30, 98.0, true)]   // bradicardia
    [InlineData(25.0, 190, 98.0, true)]   // taquicardia
    [InlineData(25.0,  70, 88.0, true)]   // hipóxia
    [InlineData(22.0,  70, 99.0, false)]  // tudo normal
    public void TelemetryReading_IsAnomaly_DeveDetectarCorretamente(
        double temp, int hr, double spo2, bool expectedAnomaly)
    {
        var reading = new TelemetryReading
        {
            Temperature = temp,
            HeartRate = hr,
            OxygenLevel = spo2
        };

        Assert.Equal(expectedAnomaly, reading.IsAnomaly());
    }

    [Fact]
    public void TelemetryReading_GetSeverity_Critico_QuandoHipoxiaSevera()
    {
        var reading = new TelemetryReading { Temperature = 22, HeartRate = 70, OxygenLevel = 82 };
        Assert.Equal(SeverityLevel.Critico, reading.GetSeverity());
    }

    [Fact]
    public void TelemetryReading_GetSeverity_Alto_QuandoTemperaturaAnomala()
    {
        var reading = new TelemetryReading { Temperature = -50, HeartRate = 70, OxygenLevel = 98 };
        Assert.Equal(SeverityLevel.Alto, reading.GetSeverity());
    }

    [Fact]
    public void TelemetryReading_GetSeverity_Normal_QuandoTudoOk()
    {
        var reading = new TelemetryReading { Temperature = 22, HeartRate = 72, OxygenLevel = 99 };
        Assert.Equal(SeverityLevel.Normal, reading.GetSeverity());
    }

    [Theory]
    [InlineData(5.0, true, 220.0, true)]   // bateria crítica
    [InlineData(50.0, false, 220.0, true)] // gerador offline
    [InlineData(50.0, true, 100.0, true)]  // sub-tensão
    [InlineData(50.0, true, 220.0, false)] // normal
    public void EnergyReading_IsAnomaly_DeveDetectarCorretamente(
        double level, bool generatorOnline, double voltage, bool expectedAnomaly)
    {
        var reading = new EnergyReading
        {
            EnergyLevel = level,
            GeneratorOnline = generatorOnline,
            VoltageV = voltage
        };

        Assert.Equal(expectedAnomaly, reading.IsAnomaly());
    }

    [Fact]
    public void EnergyReading_CreateEmergencyReading_DeveCriarLeituraAnomala()
    {
        var emergency = EnergyReading.CreateEmergencyReading("SIM-01", "Base-Test");

        Assert.True(emergency.IsAnomaly());
        Assert.False(emergency.GeneratorOnline);
        Assert.Equal(5.0, emergency.EnergyLevel);
        Assert.Equal("SIM-01", emergency.SensorId);
    }

    [Fact]
    public void TelemetryReading_GetAlertDescription_DeveConterOperatorId()
    {
        var reading = new TelemetryReading
        {
            OperatorId = "OP-999",
            Temperature = -60,
            HeartRate = 70,
            OxygenLevel = 98
        };

        var desc = reading.GetAlertDescription();
        Assert.Contains("OP-999", desc);
    }

    [Fact]
    public void SensorBase_IsDataStale_DeveRetornarTrueParaDadosAntigos()
    {
        var reading = new TelemetryReading
        {
            SensorId = "S1", OperatorId = "OP1",
            Temperature = 22, HeartRate = 70, OxygenLevel = 98,
            Timestamp = DateTime.UtcNow.AddMinutes(-10)
        };

        Assert.True(reading.IsDataStale(maxAgeMinutes: 5));
        Assert.False(reading.IsDataStale(maxAgeMinutes: 15));
    }
}
