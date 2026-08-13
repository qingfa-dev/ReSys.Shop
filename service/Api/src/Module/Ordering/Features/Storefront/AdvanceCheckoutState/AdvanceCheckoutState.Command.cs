namespace Module.Ordering.Features.Storefront.AdvanceCheckoutState;

public sealed record AdvanceCheckoutStateCommand : ICommand
{
    public Guid CartId { get; init; }
    public string TargetState { get; init; } = default!;
}
