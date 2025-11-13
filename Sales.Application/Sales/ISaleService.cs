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

    public SaleService(ISaleRepository repository)
    {
        _repository = repository;
    }

    public async Task<Sale> CreateSaleAsync(CreateSaleRequest request, CancellationToken ct = default)
    {
        var sale = Sale.Create(
            request.Number,
            request.Date,
            request.CustomerId,
            request.CustomerName,
            request.BranchId,
            request.BranchName);

        foreach (var item in request.Items)
        {
            sale.AddItem(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice);
        }

        await _repository.AddAsync(sale, ct);

        // Aqui você pode, na Application layer, publicar/logar os DomainEvents
        // (CompraEfetuada, etc.)
        return sale;
    }
}
