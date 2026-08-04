using Shared.Application.Mediators.Commands;

namespace Shared.Application.Contracts.Ordering;

public sealed record AdvanceCheckoutStateCommand : ICommand
{
    public Guid CartId { get; init; }
    public string TargetState { get; init; } = default!;
}
