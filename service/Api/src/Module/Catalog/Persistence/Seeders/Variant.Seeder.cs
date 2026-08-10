using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Domain.Variants;
using Module.Catalog.Domain.Variants.Options;
using Module.Catalog.Domain.Variants.Prices;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogVariantSeeder(IApplicationDbContext context, DemoJsonHelper jsonHelper) : AbstractDataSeeder(context)
{
    public override int Order => 132;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        if (await HasDataAsync<Variant>(cancellationToken))
            return Result.Ok();

        var json = jsonHelper.LoadIfExists<DemoVariantJson>("006_demo_variants.json");
        if (json is null)
            return Result.Ok();

        var optionValues = await Context.Set<OptionValue>().ToListAsync(cancellationToken);

        var usedSkus = new HashSet<string>(StringComparer.Ordinal);
        var existingSkus = await Context.Set<Variant>()
            .Select(v => v.Sku)
            .Where(s => s != null)
            .Cast<string>()
            .ToListAsync(cancellationToken);
        foreach (var existing in existingSkus)
        {
            usedSkus.Add(existing);
        }

        foreach (var vj in json)
        {
            var sku = vj.IsMaster ? $"MASTER-{vj.Sku}" : vj.Sku;
            var original = sku;
            var suffix = 2;
            while (!usedSkus.Add(sku))
            {
                sku = $"{original}-{suffix}";
                suffix++;
            }

            var variantResult = VariantMethod.Create(
                productId: Guid.Parse(vj.ProductId), sku: sku,
                isMaster: vj.IsMaster, position: vj.Position,
                barcode: vj.Barcode, id: Guid.Parse(vj.Id));
            var variant = variantResult.Value;
            variant.Price = vj.Price;
            variant.HsCode = vj.HsCode;
            variant.Weight = vj.Weight;
            variant.WeightUnit = vj.WeightUnit is null ? null : Enum.Parse<WeightUnit>(vj.WeightUnit);
            variant.Height = vj.Height;
            variant.Width = vj.Width;
            variant.Depth = vj.Depth;
            variant.DimensionsUnit = vj.DimensionsUnit is null ? null : Enum.Parse<DimensionUnit>(vj.DimensionsUnit);
            variant.CostPrice = vj.CostPrice;
            variant.CostCurrency = vj.CostCurrency;

            Context.Set<Variant>().Add(variant);

            var priceResult = PriceMethod.Create(amount: vj.Price, currency: "USD", variantId: variant.Id);
            var price = priceResult.Value!;
            price.IsDefault = true;
            Context.Set<Price>().Add(price);

            foreach (var ov in vj.OptionValues)
            {
                var match = optionValues.FirstOrDefault(v =>
                    v.Name.Equals(ov.OptionValueName, StringComparison.OrdinalIgnoreCase) &&
                    v.OptionTypeId == Guid.Parse(ov.OptionTypeId));
                if (match is null)
                    continue;
                Context.Set<OptionValueVariant>().Add(OptionValueVariantMethod.Create(variant.Id, match.Id).Value);
            }
        }
        await SaveChangesWithIdempotencyAsync(cancellationToken);
        return Result.Ok();
    }

    private record DemoVariantJson
    {
        public string Id { get; init; } = default!;
        public string ProductId { get; init; } = default!;
        public string Sku { get; init; } = default!;
        public bool IsMaster { get; init; }
        public int Position { get; init; }
        public decimal Price { get; init; }
        public string? Barcode { get; init; }
        public string? HsCode { get; init; }
        public decimal? Weight { get; init; }
        public string? WeightUnit { get; init; }
        public decimal? Height { get; init; }
        public decimal? Width { get; init; }
        public decimal? Depth { get; init; }
        public string? DimensionsUnit { get; init; }
        public decimal? CostPrice { get; init; }
        public string? CostCurrency { get; init; }
        public List<DemoVariantOptionJson> OptionValues { get; init; } = [];
    }

    private record DemoVariantOptionJson
    {
        public string OptionTypeId { get; init; } = default!;
        public string OptionValueName { get; init; } = default!;
    }
}
