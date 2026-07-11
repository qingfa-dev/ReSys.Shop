using Module.Payment.Services.Abstractions;
using Module.Payment.Services.Models;
using Module.Payment.Services.Gateways;
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
            PaymentMethodMethod.Create(
                "Credit Card", "credit_card", GatewayConstants.Providers.Stripe, autoCapture: true),
            PaymentMethodMethod.Create(
                "Bank Transfer", "bank_transfer", GatewayConstants.Providers.Stripe,
                displayOn: DisplayOn.Backend),
        };

        foreach (var result in methods)
            Context.Set<PaymentMethod>().Add(result.Value);

        await Context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
