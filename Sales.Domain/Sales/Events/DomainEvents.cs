using Sales.Domain.Abstractions;

namespace Sales.Domain.Sales.Events;

public sealed record SaleCreatedEvent(Guid SaleId) : IDomainEvents;
public sealed record SaleUpdatedEvent(Guid SaleId) : IDomainEvents;
public sealed record SaleCanceledEvent(Guid SaleId) : IDomainEvents;
public sealed record SaleItemCanceledEvent(Guid SaleId, Guid ItemId) : IDomainEvents;
