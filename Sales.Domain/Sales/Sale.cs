using Sales.Domain.Abstractions;
using Sales.Domain.Sales.Enums;
using Sales.Domain.Sales.Events;

namespace Sales.Domain.Sales;

public sealed class Sale : BaseEntity
{
    private readonly List<SaleItem> _items = new();

    public string Number { get; private set; } = null!;
    public DateTimeOffset Date { get; private set; }
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = null!;
    public Guid BranchId { get; private set; }
    public string BranchName { get; private set; } = null!;
    public SaleStatus Status { get; private set; }
    public decimal Total { get; private set; }

    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    private Sale() { }

    private Sale(
        string number,
        DateTimeOffset date,
        Guid customerId,
        string customerName,
        Guid branchId,
        string branchName)
    {
        Number = number;
        Date = date;
        CustomerId = customerId;
        CustomerName = customerName.Trim();
        BranchId = branchId;
        BranchName = branchName.Trim();
        Status = SaleStatus.Active;
    }

    public static Sale Create(
        string number,
        DateTimeOffset date,
        Guid customerId,
        string customerName,
        Guid branchId,
        string branchName)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Número da venda é obrigatório.", nameof(number));

        var sale = new Sale(number, date, customerId, customerName, branchId, branchName);

        sale.AddDomainEvent(new SaleCreatedEvent(sale.Id));

        return sale;
    }

    public SaleItem AddItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        EnsureNotCanceled();

        var item = new SaleItem(productId, productName, quantity, unitPrice);
        _items.Add(item);

        RecalculateTotal();
        AddDomainEvent(new SaleUpdatedEvent(Id));

        return item;
    }

    public void UpdateItemQuantity(Guid itemId, int quantity)
    {
        EnsureNotCanceled();

        var item = _items.SingleOrDefault(i => i.Id == itemId)
                   ?? throw new InvalidOperationException("Item não encontrado.");

        item.SetQuantity(quantity);

        RecalculateTotal();
        AddDomainEvent(new SaleUpdatedEvent(Id));
    }

    public void CancelSale()
    {
        if (Status == SaleStatus.Canceled)
            return;

        Status = SaleStatus.Canceled;
        AddDomainEvent(new SaleCanceledEvent(Id));
    }

    public void CancelItem(Guid itemId)
    {
        EnsureNotCanceled();

        var item = _items.SingleOrDefault(i => i.Id == itemId)
                   ?? throw new InvalidOperationException("Item não encontrado.");

        item.Cancel();
        RecalculateTotal();

        AddDomainEvent(new SaleItemCanceledEvent(Id, itemId));
    }

    private void RecalculateTotal()
    {
        Total = _items.Where(i => !i.IsCanceled)
                      .Sum(i => i.Total);
    }

    private void EnsureNotCanceled()
    {
        if (Status == SaleStatus.Canceled)
            throw new InvalidOperationException("Venda cancelada não pode ser alterada.");
    }
}
