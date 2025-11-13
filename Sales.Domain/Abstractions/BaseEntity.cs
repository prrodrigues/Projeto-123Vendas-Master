namespace Sales.Domain.Abstractions;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    private readonly List<IDomainEvents> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvents> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvents @event)
        => _domainEvents.Add(@event);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
