using MediatR;

namespace Sales.Application.Sales.Commands.FinalizeOrder;

public sealed record FinalizeOrderCommand(Guid OrderId) : IRequest<Unit>;
