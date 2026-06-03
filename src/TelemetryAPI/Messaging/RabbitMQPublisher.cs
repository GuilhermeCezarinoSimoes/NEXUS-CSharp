using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using TelemetryAPI.Interfaces;
using TelemetryAPI.Models.Structs;

namespace TelemetryAPI.Messaging;

/// <summary>
/// Publica mensagens de alerta no broker RabbitMQ quando anomalias são detectadas.
/// Cada instância abre uma conexão persistente para reduzir overhead em ambientes de alta frequência.
/// </summary>
public class RabbitMQPublisher : IMessagePublisher, IAsyncDisposable
{
    private static readonly string DefaultQueue = "nexus.alerts";

    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly ILogger<RabbitMQPublisher> _logger;

    private RabbitMQPublisher(IConnection connection, IChannel channel, ILogger<RabbitMQPublisher> logger)
    {
        _connection = connection;
        _channel = channel;
        _logger = logger;
    }

    public static async Task<RabbitMQPublisher> CreateAsync(
        IConfiguration config, ILogger<RabbitMQPublisher> logger)
    {
        var factory = new ConnectionFactory
        {
            HostName = config["RabbitMQ:Host"] ?? "localhost",
            Port = int.Parse(config["RabbitMQ:Port"] ?? "5672"),
            UserName = config["RabbitMQ:Username"] ?? "guest",
            Password = config["RabbitMQ:Password"] ?? "guest"
        };

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: DefaultQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        logger.LogInformation("RabbitMQ Publisher conectado. Fila: {Queue}", DefaultQueue);
        return new RabbitMQPublisher(connection, channel, logger);
    }

    public async Task PublishAlertAsync(AlertMessage message) =>
        await PublishAlertAsync(DefaultQueue, message);

    public async Task PublishAlertAsync(string queueName, AlertMessage message)
    {
        try
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var props = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                MessageId = message.AlertId
            };

            await _channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queueName,
                mandatory: false,
                basicProperties: props,
                body: body);

            _logger.LogInformation("Alerta publicado: {AlertId} | {Description}", message.AlertId, message.Description);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao publicar alerta no RabbitMQ: {AlertId}", message.AlertId);
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
