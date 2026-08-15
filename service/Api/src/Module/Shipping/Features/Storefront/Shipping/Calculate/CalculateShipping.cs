using Module.Shipping.Domain.Calculators;
using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Storefront.Shipping.Calculate.Shared.Mappings;
using Module.Shipping.Features.Storefront.Shipping.Calculate.Shared.Models;

using Module.Ordering.Features.Storefront.GetCartForShipping;

namespace Module.Shipping.Features.Storefront.Shipping.Calculate;
/// <summary>Calculates shipping cost for a given order and shipping method based on weight.</summary>
public static partial class CalculateShipping
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, ILogger<CommandHandler> logger, ISender sender)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Loads the shipping method and order weight via MediatR queries, then calculates cost.</summary>
        /// <param name="command">The command containing the shipping method ID and order ID.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the calculated shipping cost details or an error.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=method!=null && order!=null, post=shipping cost calculated
            _ = logger;
            var request = command.Request;

            // Check: Find the selected shipping method.
            var method = await dbContext.Set<ShippingMethod>()
                .FirstOrDefaultAsync(x => x.Id == request.ShippingMethodId && !x.IsDeleted, cancellationToken);

            // Validate: Business rules.
            if (method is null)
                return (Result<Response>)ShippingMethodResult.Errors.NotFound;

            // Check: Load cart weight/value via Ordering contract.
            var cartResult = await sender.Send(new GetCartForShippingQuery { CartId = request.OrderId }, cancellationToken);
            if (cartResult.IsFailure)
                return (Result<Response>)cartResult.Errors;
            var cart = cartResult.Value;

            // Compute: Calculate shipping cost via rate calculator.
            var calcResult = await ShippingRateCalculator.CalculateAsync(
                dbContext,
                request.ShippingMethodId,
                cart.TotalWeight,
                cart.TotalValue,
                cancellationToken);

            if (calcResult.IsFailure)
                return (Result<Response>)calcResult.Errors;

            var (cost, isFree, rateId) = ((decimal cost, bool isFree, Guid rateId))calcResult.Value;

            // Map: Return shipping cost response with method details.
            return (method, cart.Currency, cost, isFree, rateId).MapToResponse<Response>();
        }
    }
}
