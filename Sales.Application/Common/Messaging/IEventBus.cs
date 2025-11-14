namespace Sales.Application.Common.Messaging;

public interface IEventBus
{
    Task PublishAsync<T>(T message, string routingKey, CancellationToken cancellationToken = default);
}
