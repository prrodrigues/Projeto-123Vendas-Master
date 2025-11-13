using Sales.Application.Sales;
using Sales.Domain.Sales;
using System.Collections.Concurrent;

namespace Sales.Infrastructure.Persistence;

public sealed class InMemorySaleRepository : ISaleRepository
{
    private readonly ConcurrentDictionary<Guid, Sale> _db = new();

    public Task AddAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        _db.TryAdd(sale.Id, sale);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        _db.TryRemove(sale.Id, out _);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Sale>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Sale>>(_db.Values.ToList());
    }

    public Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _db.TryGetValue(id, out var sale);
        return Task.FromResult<Sale?>(sale);
    }

    public Task UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        _db[sale.Id] = sale;
        return Task.CompletedTask;
    }
}
