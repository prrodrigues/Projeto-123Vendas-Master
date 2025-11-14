using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Sales.Application.Common.Correlation;
using Sales.Application.Common.Messaging;
using System.Text;
using System.Text.Json;

namespace Sales.Infrastructure.Messaging;

public class RabbitMqEventBus : IEventBus, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqEventBus> _logger;
    private readonly ICorrelationContextAccessor _correlationAccessor;

    public RabbitMqEventBus(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqEventBus> logger,
        ICorrelationContextAccessor correlationAccessor)
    {
        _options = options.Value;
        _logger = logger;
        _correlationAccessor = correlationAccessor;

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(
            exchange: _options.Exchange,
            type: ExchangeType.Topic,
            durable: true);

        _logger.LogInformation(
            "Connected to RabbitMQ exchange {Exchange} on {Host}:{Port}",
            _options.Exchange,
            _options.HostName,
            _options.Port);
    }

    public Task PublishAsync<T>(T message, string routingKey, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Publishing message {@Message} with routing key {RoutingKey}", message, routingKey);
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var props = _channel.CreateBasicProperties();
        props.ContentType = "application/json";
        props.DeliveryMode = 2; // persistente
        props.MessageId = Guid.NewGuid().ToString();
        props.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        var correlationId = _correlationAccessor.CorrelationId ?? Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.Headers ??= new Dictionary<string, object>();
        props.Headers["x-correlation-id"] = Encoding.UTF8.GetBytes(correlationId);

        _logger.LogInformation(
            "Publishing message {MessageId} to {Exchange} with routing {RoutingKey}",
            props.MessageId,
            _options.Exchange,
            routingKey);
        props.DeliveryMode = 2; // persistence

        _channel.BasicPublish(
            exchange: _options.Exchange,
            routingKey: routingKey,
            basicProperties: props,
            body: body);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing RabbitMQ resources");
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
