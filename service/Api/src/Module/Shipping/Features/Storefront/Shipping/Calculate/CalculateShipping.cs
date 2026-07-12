using Module.Ordering.Domain.Orders;
using Module.Shipping.Domain.Calculators;
using Module.Shipping.Domain.ShippingMethods;

namespace Module.Shipping.Features.Storefront.Shipping.Calculate;
/// <summary>Calculates shipping cost for a given order and shipping method based on weight.</summary>
public static partial class CalculateShipping
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, ILogger<CommandHandler> logger)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Loads the shipping method and order, computes order weight from line items, then calculates cost.</summary>
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

            // Check: Load order with line items to compute weight.
            var order = await dbContext.Set<Order>()
                .Include(o => o.LineItems)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order is null)
                return (Result<Response>)OrderResult.Errors.NotFound(request.OrderId);

            // Compute: Calculate order weight from line items.
            var variantIds = order.LineItems.Select(li => li.VariantId).Distinct().ToList();
            var variantWeights = await dbContext.Set<Catalog.Domain.Products.Variants.Variant>()
                .Where(v => variantIds.Contains(v.Id))
                .Select(v => new { v.Id, v.Weight })
                .ToListAsync(cancellationToken);

            var weightMap = variantWeights.ToDictionary(v => v.Id, v => v.Weight ?? 0m);
            var orderWeight = order.LineItems.Sum(li =>
                weightMap.TryGetValue(li.VariantId, out var w) ? li.Quantity * w : 0m);

            // Compute: Calculate shipping cost via rate calculator.
            var calcResult = await ShippingRateCalculator.CalculateAsync(
                dbContext,
                request.ShippingMethodId,
                orderWeight,
                order.Total,
                cancellationToken);

            if (calcResult.IsFailure)
                return (Result<Response>)calcResult.Errors;

            var (cost, isFree) = calcResult.Value;

            // Map: Return shipping cost response with method details.
            return new Response
            {
                ShippingMethodId = method.Id,
                MethodName = method.Name,
                Cost = cost,
                Currency = order?.Currency ?? "USD",
                IsFreeShipping = isFree
            };
        }
    }
}
