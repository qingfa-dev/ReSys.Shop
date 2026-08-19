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
            ShippingMethodMethod.Create("Standard Shipping", "FlatRate", code: "standard"),
            ShippingMethodMethod.Create("Express Shipping", "FlatRate", code: "express"),
            ShippingMethodMethod.Create("Free Shipping", "FreeShipping", code: "free"),
        };

        foreach (var result in methods)
        {
            var method = result.Value;
            // Seed: Default worldwide zone ("*") so all methods remain available until zones are curated.
            Context.Set<ShippingMethod>().Add(method);
        }

        await SaveChangesWithIdempotencyAsync(cancellationToken);

        return Result.Ok();
    }
}