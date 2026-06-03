using TelemetryAPI.Interfaces;
using TelemetryAPI.Models.Structs;

namespace TelemetryAPI.Messaging;

/// <summary>
/// Implementação nula do publisher — usada quando RabbitMQ não está disponível.
/// Garante que a API funcione normalmente em ambiente de desenvolvimento sem broker.
/// Padrão Null Object: evita NullReferenceException e mantém o fluxo intacto.
/// </summary>
public class NullMessagePublisher : IMessagePublisher
{
    private readonly ILogger<RabbitMQPublisher> _logger;

    public NullMessagePublisher(ILogger<RabbitMQPublisher> logger) => _logger = logger;

    public Task PublishAlertAsync(AlertMessage message)
    {
        _logger.LogWarning("[NullPublisher] Alerta NÃO enviado ao broker (RabbitMQ offline): {Alert}",
            message.ToString());
        return Task.CompletedTask;
    }

    public Task PublishAlertAsync(string queueName, AlertMessage message)
        => PublishAlertAsync(message);
}
