using Module.Payment.Domain.PaymentMethods;

namespace Module.Payment.Persistence.Seeders;

public sealed class PaymentMethodSeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
{
    public override int Order => 160;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasData = await HasDataAsync<PaymentMethod>(cancellationToken);
        if (hasData)
            return Result.Ok();

        var methods = new[]
        {
            PaymentMethodExtensions.Create("Credit Card", "credit_card", "CreditCard", autoCapture: true),
            PaymentMethodExtensions.Create("PayPal", "paypal", "PayPal"),
            PaymentMethodExtensions.Create("Bank Transfer", "bank_transfer", "BankTransfer", displayOn: DisplayOn.Backend),
        };

        foreach (var result in methods)
            Context.Set<PaymentMethod>().Add(result.Value);

        await Context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
