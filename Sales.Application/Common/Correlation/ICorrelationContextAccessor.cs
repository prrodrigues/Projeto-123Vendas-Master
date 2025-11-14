using System.Threading;

namespace Sales.Application.Common.Correlation;

public interface ICorrelationContextAccessor
{
    string? CorrelationId { get; set; }
}

public sealed class CorrelationContextAccessor : ICorrelationContextAccessor
{
    private static readonly AsyncLocal<string?> CorrelationIdSlot = new();

    public string? CorrelationId
    {
        get => CorrelationIdSlot.Value;
        set => CorrelationIdSlot.Value = value;
    }
}
