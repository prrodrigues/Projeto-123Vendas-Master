using System.Linq;
using MediatR;
using Sales.Application.Common.Messaging;
using Sales.Application.Sales.IntegrationEvents;
using Sales.Domain.Orders;

namespace Sales.Application.Sales.Commands.FinalizeOrder;

public sealed class FinalizeOrderCommandHandler : IRequestHandler<FinalizeOrderCommand, Unit>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEventBus _eventBus;

    public FinalizeOrderCommandHandler(IOrderRepository orderRepository, IEventBus eventBus)
    {
        _orderRepository = orderRepository;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(FinalizeOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            throw new InvalidOperationException("Pedido não encontrado.");

        order.Finalize(); // regra de domínio

        await _orderRepository.UpdateAsync(order, cancellationToken);

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

        await _eventBus.PublishAsync(evt, "sales.order.finalized", cancellationToken);

        return Unit.Value;
    }
}
