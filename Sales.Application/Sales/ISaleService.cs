using Microsoft.Extensions.Logging;
using Sales.Application.Sales.Dtos;
using Sales.Domain.Sales;

namespace Sales.Application.Sales;

public interface ISaleService
{
    Task<Sale> CreateSaleAsync(CreateSaleRequest request, CancellationToken ct = default);
    // outros métodos: Update, Get, Cancel, etc.
}

public sealed class SaleService : ISaleService
{
    private readonly ISaleRepository _repository;
    private readonly ILogger<SaleService> _logger;

    public SaleService(ISaleRepository repository, ILogger<SaleService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Sale> CreateSaleAsync(CreateSaleRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Creating sale {SaleNumber} for customer {CustomerId}",
            request.Number,
            request.CustomerId);

        var sale = Sale.Create(
            request.Number,
            request.Date,
            request.CustomerId,
            request.CustomerName,
            request.BranchId,
            request.BranchName);

        var totalItems = request.Items.Count;
        _logger.LogDebug("Adding {ItemCount} items to sale {SaleNumber}", totalItems, request.Number);

        foreach (var item in request.Items)
        {
            _logger.LogDebug(
                "Adding item {ProductId} (qty {Quantity}) to sale {SaleNumber}",
                item.ProductId,
                item.Quantity,
                request.Number);

            sale.AddItem(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice);
        }

        await _repository.AddAsync(sale, ct);

        _logger.LogInformation(
            "Sale {SaleId} created successfully with total {SaleTotal}",
            sale.Id,
            sale.Total);

        // Aqui você pode, na Application layer, publicar/logar os DomainEvents
        // (CompraEfetuada, etc.)
        return sale;
    }
}
