using System.Linq;
using MediatR;
using Microsoft.Extensions.Logging;
using Sales.Application.Common.Messaging;
using Sales.Application.Sales.IntegrationEvents;
using Sales.Domain.Orders;
using System.Linq;

namespace Sales.Application.Sales.Commands.FinalizeOrder;

public sealed class FinalizeOrderCommandHandler : IRequestHandler<FinalizeOrderCommand, Unit>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<FinalizeOrderCommandHandler> _logger;

    public FinalizeOrderCommandHandler(IOrderRepository orderRepository, IEventBus eventBus,
        ILogger<FinalizeOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<Unit> Handle(FinalizeOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Finalizing order {OrderId}", request.OrderId);

        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            _logger.LogWarning("Order {OrderId} not found", request.OrderId);
            throw new InvalidOperationException("Pedido não encontrado.");
        }

        _logger.LogInformation("Order {OrderId} finalized", request.OrderId);
        order.Complete(); // domain rule validation inside

        await _orderRepository.UpdateAsync(order, cancellationToken);

        _logger.LogInformation("Order {OrderId} finalized successfully", order.Id);

        var evt = new OrderFinalizedIntegrationEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            Total = order.CalculateTotal(),
            FinalizedAt = DateTime.UtcNow,
            Items = order.Items.Select(i => new OrderFinalizedIntegrationEvent.OrderItemDto
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToArray()
        };

        _logger.LogInformation(
            "Publishing order finalized event for order {OrderId} with {ItemCount} items",
            order.Id,
            evt.Items.Count);

        await _eventBus.PublishAsync(evt, "sales.order.finalized", cancellationToken);

        return Unit.Value;
    }
}
