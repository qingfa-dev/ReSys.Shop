using Module.Ordering.Domain.Orders;
using Module.Payment.Domain.PaymentMethods;
using Module.Payment.Domain.Payments;
using PaymentEntity = Module.Payment.Domain.Payments.PaymentRecord;

namespace Module.Ordering.Persistence.Seeders;

public sealed class PaymentSeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
{
    public override int Order => 200;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasData = await HasDataAsync<PaymentEntity>(cancellationToken);
        if (hasData)
            return Result.Ok();

        var orders = await Context.Set<Order>()
            .Where(o => o.Status == OrderStatus.Placed)
            .ToListAsync(cancellationToken);

        var creditCard = await Context.Set<PaymentMethod>()
            .FirstOrDefaultAsync(pm => pm.Code == "credit_card", cancellationToken);

        if (orders.Count == 0 || creditCard is null)
            return Result.Ok();

        foreach (var order in orders)
        {
            var paymentResult = PaymentFactory.Create(order.PaymentTotal, creditCard.Id, order.Id);
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
