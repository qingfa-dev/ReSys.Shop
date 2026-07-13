using Module.Shipping.Domain.ShippingMethods;

namespace Module.Shipping.Persistence.Seeders;

public sealed class ShippingMethodSeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
{
    public override int Order => 170;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasData = await HasDataAsync<ShippingMethod>(cancellationToken);
        if (hasData)
            return Result.Ok();

        var methods = new[]
        {
            ShippingMethodExtensions.Create("Standard Shipping", "FlatRate", code: "standard"),
            ShippingMethodExtensions.Create("Express Shipping", "FlatRate", code: "express"),
            ShippingMethodExtensions.Create("Free Shipping", "FreeShipping", code: "free"),
        };

        foreach (var result in methods)
            Context.Set<ShippingMethod>().Add(result.Value);

        await Context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}