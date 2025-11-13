namespace Sales.Application.Sales.Dtos;

public sealed record SaleItemDto(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);

public sealed record CreateSaleRequest(
    string Number,
    DateTimeOffset Date,
    Guid CustomerId,
    string CustomerName,
    Guid BranchId,
    string BranchName,
    List<SaleItemDto> Items);

public sealed record SaleResponse(
    Guid Id,
    string Number,
    DateTimeOffset Date,
    Guid CustomerId,
    string CustomerName,
    Guid BranchId,
    string BranchName,
    decimal Total,
    string Status,
    IReadOnlyCollection<SaleItemResponse> Items);

public sealed record SaleItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal DiscountPercent,
    decimal Total,
    bool IsCanceled);
