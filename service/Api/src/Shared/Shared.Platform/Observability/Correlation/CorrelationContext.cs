namespace Shared.Observability.Correlation;

internal sealed class CorrelationContext : ICorrelationContext
{
    public string? CorrelationId { get; set; }
}
