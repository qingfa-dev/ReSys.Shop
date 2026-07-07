using Microsoft.EntityFrameworkCore;
using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.Cart.ChangeCurrency;

/// <summary>Changes the cart currency and homogenizes line item currencies.</summary>
public static partial class ChangeCartCurrency
{
    public class Request
    {
        public string Currency { get; init; } = string.Empty;
    }

    public class Response
    {
        public Guid Id { get; init; }
        public string Currency { get; init; } = string.Empty;
        public DateTimeOffset? ModifiedAtUtc { get; init; }
    }

    public sealed record Command(Guid OrderId, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var order = await dbContext.Set<Order>()
                .Include(o => o.LineItems)
                .FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);

            if (order is null)
                return (Result<Response>)OrderResult.Errors.NotFound(command.OrderId);

            if (order.Status != OrderStatus.Draft)
                return (Result<Response>)OrderResult.Errors.InvalidStatusTransition;

            order.Currency = command.Request.Currency;
            order.HomogenizeLineItemCurrencies();
            order.ModifiedAtUtc = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);

            return new Response
            {
                Id = order.Id,
                Currency = order.Currency,
                ModifiedAtUtc = order.ModifiedAtUtc
            };
        }
    }
}
