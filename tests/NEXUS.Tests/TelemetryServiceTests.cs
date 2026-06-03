using Microsoft.Extensions.Logging;
using Moq;
using TelemetryAPI.Interfaces;
using TelemetryAPI.Models;
using TelemetryAPI.Models.Structs;
using TelemetryAPI.Services;

namespace NEXUS.Tests;

/// <summary>
/// Testes unitários do TelemetryService.
/// Usamos Moq para simular ITelemetryRepository e IMessagePublisher,
/// isolando a lógica de negócio da infraestrutura real (banco + RabbitMQ).
/// </summary>
public class TelemetryServiceTests
{
    private readonly Mock<ITelemetryRepository> _repoMock;
    private readonly Mock<IEnergyRepository> _energyRepoMock;
    private readonly Mock<IMessagePublisher> _publisherMock;
    private readonly TelemetryService _service;

    public TelemetryServiceTests()
    {
        _repoMock = new Mock<ITelemetryRepository>();
        _energyRepoMock = new Mock<IEnergyRepository>();
        _publisherMock = new Mock<IMessagePublisher>();
        var logger = Mock.Of<ILogger<TelemetryService>>();

        _service = new TelemetryService(
            _repoMock.Object,
            _energyRepoMock.Object,
            _publisherMock.Object,
            logger);
    }

    [Fact]
    public async Task ProcessHealthReading_Anomalia_DevePublicarAlerta()
    {
        // Arrange
        var reading = new TelemetryReading
        {
            SensorId = "SENSOR-01",
            OperatorId = "OP-001",
            BaseLocation = "Base-Antarctica",
            Temperature = -60.0,  // abaixo do limiar crítico de -40°C
            HeartRate = 75,
            OxygenLevel = 98.0
        };

        _repoMock.Setup(r => r.AddAsync(It.IsAny<TelemetryReading>()))
                 .ReturnsAsync(reading);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<TelemetryReading>()))
                 .Returns(Task.CompletedTask);
        _publisherMock.Setup(p => p.PublishAlertAsync(It.IsAny<AlertMessage>()))
                      .Returns(Task.CompletedTask);

        // Act
        await _service.ProcessHealthReadingAsync(reading);

        // Assert — publisher deve ter sido chamado exatamente uma vez pois há anomalia
        _publisherMock.Verify(p => p.PublishAlertAsync(It.IsAny<AlertMessage>()), Times.Once);
    }

    [Fact]
    public async Task ProcessHealthReading_Leitura_Normal_NaoDevePublicarAlerta()
    {
        // Arrange
        var reading = new TelemetryReading
        {
            SensorId = "SENSOR-02",
            OperatorId = "OP-002",
            BaseLocation = "Base-Offshore-Beta",
            Temperature = 22.0,
            HeartRate = 70,
            OxygenLevel = 99.0
        };

        _repoMock.Setup(r => r.AddAsync(It.IsAny<TelemetryReading>()))
                 .ReturnsAsync(reading);

        // Act
        await _service.ProcessHealthReadingAsync(reading);

        // Assert — nenhum alerta deve ser publicado para leitura normal
        _publisherMock.Verify(p => p.PublishAlertAsync(It.IsAny<AlertMessage>()), Times.Never);
    }

    [Fact]
    public async Task ProcessHealthReading_SensorIdVazio_DeveLancarArgumentException()
    {
        // Arrange
        var reading = new TelemetryReading
        {
            SensorId = "",  // inválido
            OperatorId = "OP-001",
            Temperature = 25.0,
            HeartRate = 80,
            OxygenLevel = 98.0
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.ProcessHealthReadingAsync(reading));
    }

    [Fact]
    public async Task ProcessHealthReading_TemperaturaImpossivel_DeveLancarArgumentOutOfRangeException()
    {
        // Arrange: temperatura acima de 1000°C é fisicamente impossível
        var reading = new TelemetryReading
        {
            SensorId = "SENSOR-03",
            OperatorId = "OP-003",
            Temperature = 5000.0,
            HeartRate = 70,
            OxygenLevel = 98.0
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _service.ProcessHealthReadingAsync(reading));
    }

    [Fact]
    public async Task GetHistoryByOperator_DatasInvertidas_DeveLancarArgumentException()
    {
        // Arrange
        var from = DateTime.UtcNow;
        var to = DateTime.UtcNow.AddHours(-2);  // 'to' antes de 'from' — inválido

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GetHistoryByOperatorAsync("OP-001", from, to));
    }

    [Fact]
    public async Task ProcessEnergyReading_SemGerador_DevePublicarAlerta()
    {
        // Arrange: gerador offline é anomalia crítica
        var reading = new EnergyReading
        {
            SensorId = "ENERGY-01",
            BaseLocation = "Base-Antarctica",
            EnergyLevel = 45.0,
            VoltageV = 220.0,
            GeneratorOnline = false
        };

        _energyRepoMock.Setup(r => r.AddAsync(It.IsAny<EnergyReading>()))
                       .ReturnsAsync(reading);
        _publisherMock.Setup(p => p.PublishAlertAsync(It.IsAny<AlertMessage>()))
                      .Returns(Task.CompletedTask);

        // Act
        await _service.ProcessEnergyReadingAsync(reading);

        // Assert
        _publisherMock.Verify(p => p.PublishAlertAsync(It.IsAny<AlertMessage>()), Times.Once);
    }

    [Fact]
    public async Task ProcessHealthReading_Hipoxia_DevePublicarAlertaComSeveridadeCritica()
    {
        // Arrange: SpO2 < 85% é situação crítica de risco de vida
        var reading = new TelemetryReading
        {
            SensorId = "BIO-01",
            OperatorId = "OP-010",
            BaseLocation = "Base-Military-Charlie",
            Temperature = 20.0,
            HeartRate = 110,
            OxygenLevel = 82.0  // hipóxia severa
        };

        _repoMock.Setup(r => r.AddAsync(It.IsAny<TelemetryReading>()))
                 .ReturnsAsync(reading);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<TelemetryReading>()))
                 .Returns(Task.CompletedTask);

        AlertMessage capturedAlert = default;
        _publisherMock.Setup(p => p.PublishAlertAsync(It.IsAny<AlertMessage>()))
                      .Callback<AlertMessage>(msg => capturedAlert = msg)
                      .Returns(Task.CompletedTask);

        // Act
        await _service.ProcessHealthReadingAsync(reading);

        // Assert
        Assert.Equal("CRITICO", capturedAlert.Severity);
        Assert.Equal("HEALTH", capturedAlert.SensorType);
    }
}
