using Microsoft.Extensions.Logging;
using Sales.Application.Sales;
using Sales.Domain.Sales;
using System.Collections.Concurrent;
using System.Linq;

namespace Sales.Infrastructure.Persistence;

public sealed class InMemorySaleRepository : ISaleRepository
{
    private readonly ConcurrentDictionary<Guid, Sale> _db = new();
    private readonly ILogger<InMemorySaleRepository> _logger;

    public InMemorySaleRepository(ILogger<InMemorySaleRepository> logger)
    {
        _logger = logger;
    }

    public Task AddAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        _db.TryAdd(sale.Id, sale);
        _logger.LogInformation("Sale {SaleId} persisted in memory", sale.Id);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        var removed = _db.TryRemove(sale.Id, out _);
        _logger.LogInformation("Sale {SaleId} removed from memory: {Removed}", sale.Id, removed);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Sale>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var values = _db.Values.ToList();
        _logger.LogDebug("Retrieved {Count} sales from memory", values.Count);
        return Task.FromResult<IReadOnlyList<Sale>>(values);
    }

    public Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var found = _db.TryGetValue(id, out var sale);
        _logger.LogDebug("Lookup for sale {SaleId} found: {Found}", id, found);
        return Task.FromResult<Sale?>(sale);
    }

    public Task UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        _db[sale.Id] = sale;
        _logger.LogInformation("Sale {SaleId} updated in memory", sale.Id);
        return Task.CompletedTask;
    }
}
