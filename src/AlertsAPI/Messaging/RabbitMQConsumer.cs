using System.Text;
using System.Text.Json;
using AlertsAPI.Models.Structs;
using AlertsAPI.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AlertsAPI.Messaging;

/// <summary>
/// Serviço de background que consome alertas da fila RabbitMQ publicados pela TelemetryAPI.
/// Implementa IHostedService para iniciar junto com a aplicação.
/// </summary>
public class RabbitMQConsumer : BackgroundService
{
    private static readonly string QueueName = "nexus.alerts";

    private readonly IConfiguration _config;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMQConsumer> _logger;

    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMQConsumer(
        IConfiguration config,
        IServiceProvider serviceProvider,
        ILogger<RabbitMQConsumer> logger)
    {
        _config = config;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await ConnectAsync(stoppingToken);
            _logger.LogInformation("RabbitMQ Consumer aguardando mensagens na fila: {Queue}", QueueName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "RabbitMQ indisponível — Consumer não iniciado. " +
                "AlertsAPI continua funcionando via endpoints REST.");
            return;
        }

        stoppingToken.Register(() =>
            _logger.LogInformation("Consumer cancelado — encerrando conexão RabbitMQ."));

        // Mantém o consumer ativo enquanto a aplicação estiver rodando
        await Task.Delay(Timeout.Infinite, stoppingToken).ContinueWith(_ => { });
    }

    private async Task ConnectAsync(CancellationToken ct)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config["RabbitMQ:Host"] ?? "localhost",
            Port = int.Parse(_config["RabbitMQ:Port"] ?? "5672"),
            UserName = _config["RabbitMQ:Username"] ?? "guest",
            Password = _config["RabbitMQ:Password"] ?? "guest"
        };

        _connection = await factory.CreateConnectionAsync(ct);
        _channel = await _connection.CreateChannelAsync(cancellationToken: ct);

        await _channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: ct);

        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 10, global: false, cancellationToken: ct);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageReceivedAsync;

        await _channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: ct);
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        var body = ea.Body.ToArray();
        var json = Encoding.UTF8.GetString(body);

        try
        {
            var payload = JsonSerializer.Deserialize<AlertPayload>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            _logger.LogInformation("Mensagem recebida: {AlertId} | {Severity}",
                payload.AlertId, payload.Severity);

            using var scope = _serviceProvider.CreateScope();
            var alertService = scope.ServiceProvider.GetRequiredService<AlertService>();
            await alertService.RegisterAlertAsync(payload);

            await _channel!.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Mensagem com formato inválido descartada: {Body}", json);
            await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem. Recolocando na fila: {AlertId}",
                ea.DeliveryTag);
            await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        if (_channel != null) await _channel.DisposeAsync();
        if (_connection != null) await _connection.DisposeAsync();
    }
}
