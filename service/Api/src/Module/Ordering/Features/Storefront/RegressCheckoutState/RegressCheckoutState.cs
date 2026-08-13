namespace Module.Ordering.Features.Storefront.RegressCheckoutState;

public sealed record RegressCheckoutStateCommand : ICommand
{
    public Guid CartId { get; init; }
    public string TargetState { get; init; } = default!;
}
