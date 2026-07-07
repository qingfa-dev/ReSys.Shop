using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.Cart.CreateCart;

/// <summary>Creates a new shopping cart for the current user or as a guest cart.</summary>
public static partial class CreateCart
{
    public class Response
    {
        public Guid Id { get; init; }
        public string Number { get; init; } = string.Empty;
        public string Currency { get; init; } = string.Empty;
        public DateTimeOffset CreatedAtUtc { get; init; }
    }

    public sealed record Command : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var userId = Guid.TryParse(currentUser.UserId, out var parsedId) ? parsedId : (Guid?)null;
            var storeId = Guid.Empty; // Resolved from context or default
            var sessionId = currentUser.IsAuthenticated ? null : currentUser.SessionId;

            var createResult = OrderExtensions.Create("USD", userId, storeId, sessionId: sessionId);
            if (createResult.IsFailure) return (Result<Response>)createResult.Failures;

            var order = createResult.Value;
            dbContext.Set<Order>().Add(order);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new Response
            {
                Id = order.Id,
                Number = order.Number,
                Currency = order.Currency,
                CreatedAtUtc = order.CreatedAtUtc
            };
        }
    }
}
