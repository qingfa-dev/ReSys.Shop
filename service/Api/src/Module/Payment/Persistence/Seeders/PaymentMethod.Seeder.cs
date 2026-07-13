using Module.Payment.Services.Models;
using Module.Payment.Domain.PaymentMethods;

namespace Module.Payment.Persistence.Seeders;

// Initialize: Seed default payment methods on first run — runs at order 160
public sealed class PaymentMethodSeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
{
    public override int Order => 160;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        // Check: Skip if data already exists
        var hasData = await HasDataAsync<PaymentMethod>(cancellationToken);
        if (hasData)
            return Result.Ok();

        // Create: Default payment methods — Credit Card (Stripe), Bank Transfer, Test Card (Bogus)
        var methods = new[]
        {
            PaymentMethodMethod.Create(
                "Credit Card", "credit_card", GatewayConstants.Providers.Stripe, autoCapture: true),
            PaymentMethodMethod.Create(
                "Bank Transfer", "bank_transfer", GatewayConstants.Providers.Stripe,
                displayOn: DisplayOn.Backend),
            PaymentMethodMethod.Create(
                "Test Card (Bogus)", "bogus_card", GatewayConstants.Providers.Bogus,
                autoCapture: false),
        };

        foreach (var result in methods)
            Context.Set<PaymentMethod>().Add(result.Value);

        await Context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}