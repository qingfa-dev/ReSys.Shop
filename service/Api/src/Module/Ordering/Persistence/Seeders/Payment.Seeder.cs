using Module.Ordering.Domain.Orders;
using Module.Payment.Domain.PaymentMethods;
using Module.Payment.Domain.PaymentCaptures;

using PaymentEntity = Module.Payment.Domain.PaymentCaptures.PaymentCapture;

namespace Module.Ordering.Persistence.Seeders;

// Initialize: Seed payment records for placed orders that lack payment data in development databases
public sealed class PaymentSeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
{
    public override int Order => 200;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        // Check: Skip seeding if payment data already exists
        var hasData = await HasDataAsync<PaymentEntity>(cancellationToken);
        if (hasData)
            return Result.Ok();

        // Acquire: Fetch placed orders and payment method reference needed for payment creation
        var orders = await Context.Set<Order>()
            .Where(o => o.Status == OrderStatus.Placed)
            .ToListAsync(cancellationToken);

        var creditCard = await Context.Set<PaymentMethod>()
            .FirstOrDefaultAsync(pm => pm.Code == "credit_card", cancellationToken);

        // Validate: Skip seeding if no placed orders or payment method found
        if (orders.Count == 0 || creditCard is null)
            return Result.Ok();

        foreach (var order in orders)
        {
            // Create: Payment capture with full process-and-complete lifecycle for each placed order
            var paymentResult = PaymentCaptureMethod.Create(order.PaymentTotal, creditCard.Id, order.Id);
            if (paymentResult.IsFailure)
                continue;

            var payment = paymentResult.Value;
            if (payment.Process().IsFailure)
                continue;
            if (payment.Complete().IsFailure)
                continue;

            Context.Set<PaymentEntity>().Add(payment);
        }

        await Context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}