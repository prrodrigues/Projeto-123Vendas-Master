namespace Sales.Domain.Orders;

public class Order
{
    private readonly List<OrderItem> _items = new();

    public Guid Id { get; }
    public Guid CustomerId { get; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public bool IsFinalized { get; private set; }

    public Order(Guid customerId)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId cannot be empty.", nameof(customerId));

        Id = Guid.NewGuid();
        CustomerId = customerId;
    }

    public void AddItem(Guid productId, int quantity, decimal unitPrice)
    {
        if (IsFinalized)
            throw new InvalidOperationException("Cannot add items to a finalized order.");

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        if (unitPrice <= 0)
            throw new ArgumentException("Unit price must be greater than zero.", nameof(unitPrice));

        _items.Add(new OrderItem(productId, quantity, unitPrice));
    }

    public decimal CalculateTotal()
        => _items.Sum(i => i.Quantity * i.UnitPrice);

    public void Complete()
    {
        if (!_items.Any())
            throw new InvalidOperationException("Cannot finalize an order with no items.");

        IsFinalized = true;
    }
}
