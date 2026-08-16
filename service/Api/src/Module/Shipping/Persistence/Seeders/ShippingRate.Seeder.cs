using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Domain.ShippingRates;

namespace Module.Shipping.Persistence.Seeders;

public sealed class ShippingRateSeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
{
    public override int Order => 180;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasData = await HasDataAsync<ShippingRate>(cancellationToken);
        if (hasData)
            return Result.Ok();

        var standard = await Context.Set<ShippingMethod>().FirstOrDefaultAsync(sm => sm.Code == "standard", cancellationToken);
        var express = await Context.Set<ShippingMethod>().FirstOrDefaultAsync(sm => sm.Code == "express", cancellationToken);
        var free = await Context.Set<ShippingMethod>().FirstOrDefaultAsync(sm => sm.Code == "free", cancellationToken);

        if (standard is null || express is null || free is null)
            return Result.Ok();

        var rates = new[]
        {
            ShippingRateMethod.Create("Standard", 5.99m, standard.Id, deliveryRange: "5-7 business days"),
            ShippingRateMethod.Create("Express", 14.99m, express.Id, deliveryRange: "2-3 business days"),
            ShippingRateMethod.Create("Free Shipping", 9.99m, free.Id, deliveryRange: "7-14 business days", freeShippingThreshold: 100m),
        };

        foreach (var result in rates)
            Context.Set<ShippingRate>().Add(result.Value);

        await SaveChangesWithIdempotencyAsync(cancellationToken);

        return Result.Ok();
    }
}