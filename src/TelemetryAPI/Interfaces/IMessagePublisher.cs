using TelemetryAPI.Models.Structs;

namespace TelemetryAPI.Interfaces;

public interface IMessagePublisher
{
    Task PublishAlertAsync(AlertMessage message);
    Task PublishAlertAsync(string queueName, AlertMessage message);
}
