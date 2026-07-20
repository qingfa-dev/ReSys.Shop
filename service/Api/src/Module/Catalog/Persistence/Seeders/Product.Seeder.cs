using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Classifications;
using Module.Catalog.Domain.Products.Options;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Domain.Products.Variants.Options;
using Module.Catalog.Domain.Products.Variants.Prices;
using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.OptionTypes.Values;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogDemoSeeder(IApplicationDbContext context, DemoJsonHelper jsonHelper) : AbstractDataSeeder(context)
{
    public override int Order => 130;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasProducts = await HasDataAsync<Product>(cancellationToken);
        if (hasProducts)
            return Result.Ok();

        var jsonProducts = jsonHelper.LoadIfExists<DemoProductJson>("demo_products.json");
        var jsonVariants = jsonHelper.LoadIfExists<DemoVariantJson>("demo_variants.json");
        var jsonImages = jsonHelper.LoadIfExists<DemoVariantImageJson>("demo_variant_images.json");
        var jsonAssignments = jsonHelper.LoadIfExists<DemoOptionAssignmentJson>("demo_option_assignments.json");
        var jsonClassifications = jsonHelper.LoadIfExists<DemoClassificationJson>("demo_classifications.json");

        if (jsonProducts is null || jsonVariants is null)
            return Result.Ok();

        await SeedFromJsonAsync(jsonProducts, jsonVariants, jsonImages, jsonAssignments, jsonClassifications, cancellationToken);
        return Result.Ok();
    }

    private async Task SeedFromJsonAsync(
        DemoProductJson[] products, DemoVariantJson[] variants,
        DemoVariantImageJson[]? images, DemoOptionAssignmentJson[]? assignments,
        DemoClassificationJson[]? classifications, CancellationToken ct)
    {
        var optionValues = await Context.Set<OptionValue>().ToListAsync(ct);
        var optionTypes = await Context.Set<OptionType>().ToListAsync(ct);

        var colorTypeId = optionTypes.FirstOrDefault(o => o.Name == "Color")?.Id;
        var sizeTypeId = optionTypes.FirstOrDefault(o => o.Name == "Size")?.Id;

        var productIds = products.Select(p => Guid.Parse(p.Id)).ToArray();
        var existingProductIds = await Context.Set<Product>()
            .Where(p => productIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToHashSetAsync(ct);

        var existingProductOptionTypes = new HashSet<(Guid ProductId, Guid OptionTypeId)>(
            (await Context.Set<ProductOptionType>().ToListAsync(ct))
            .Select(pot => (pot.ProductId, pot.OptionTypeId)));

        foreach (var pj in products)
        {
            var pid = Guid.Parse(pj.Id);
            if (existingProductIds.Contains(pid))
                continue;

            var productResult = ProductMethod.Create(
                name: pj.Name, slug: pj.Slug, description: pj.Description,
                status: ProductStatus.Active, availableOn: DateTimeOffset.UtcNow,
                metaTitle: pj.MetaTitle, metaDescription: pj.Description,
                metaKeywords: pj.MetaKeywords, id: pid);
            var product = productResult.Value;
            product.GenderTarget = pj.GenderTarget;

            product.MasterVariantId = Guid.Parse(pj.MasterVariantId);

            product.StyleCode = pj.StyleCode;
            product.SeasonName = pj.SeasonName;
            product.MaterialComposition = pj.MaterialComposition;
            product.CareInstructions = pj.CareInstructions;
            product.Department = pj.Department;

            Context.Set<Product>().Add(product);

            if (colorTypeId is not null && sizeTypeId is not null)
            {
                if (!existingProductOptionTypes.Contains((pid, colorTypeId.Value)))
                    AddProductOptionType(pid, colorTypeId.Value, 0);

                if (!existingProductOptionTypes.Contains((pid, sizeTypeId.Value)))
                    AddProductOptionType(pid, sizeTypeId.Value, 1);
            }
        }
        await Context.SaveChangesAsync(ct);

        var variantIds = variants.Select(v => Guid.Parse(v.Id)).ToArray();
        var existingVariantIds = await Context.Set<Variant>()
            .Where(v => variantIds.Contains(v.Id))
            .Select(v => v.Id)
            .ToHashSetAsync(ct);

        foreach (var vj in variants)
        {
            var vid = Guid.Parse(vj.Id);
            if (existingVariantIds.Contains(vid))
                continue;

            var variantResult = VariantMethod.Create(
                productId: Guid.Parse(vj.ProductId), sku: vj.Sku,
                isMaster: vj.IsMaster, position: vj.Position,
                barcode: vj.Barcode, id: vid);
            var variant = variantResult.Value;
            variant.Price = vj.Price;
            variant.HsCode = vj.HsCode;

            var priceResult = PriceMethod.Create(amount: vj.Price, currency: "USD", variantId: variant.Id);
            var price = priceResult.Value!;
            price.IsDefault = true;

            Context.Set<Variant>().Add(variant);
            Context.Set<Price>().Add(price);
        }
        await Context.SaveChangesAsync(ct);

        if (images is not null)
        {
            var imageIds = images.Select(img => Guid.Parse(img.Id)).ToArray();
            var existingImageIds = await Context.Set<VariantImage>()
                .Where(vi => imageIds.Contains(vi.Id))
                .Select(vi => vi.Id)
                .ToHashSetAsync(ct);

            foreach (var img in images)
            {
                var imgId = Guid.Parse(img.Id);
                if (existingImageIds.Contains(imgId))
                    continue;

                var type = img.Type == "Search" ? VariantImageType.Search : VariantImageType.Default;
                var imgResult = VariantImageMethod.Create(
                    contentType: img.ContentType, fileName: img.FileName,
                    fileSize: 0, url: string.Empty, storagePath: img.StoragePath,
                    position: img.Position, alt: img.Alt, type: type,
                    variantId: Guid.Parse(img.VariantId));
                var image = imgResult.Value;
                image.Id = imgId;
                Context.Set<VariantImage>().Add(image);
            }
            await Context.SaveChangesAsync(ct);
        }

        if (assignments is not null)
        {
            var existingAssignments = new HashSet<(Guid VariantId, Guid OptionValueId)>(
                (await Context.Set<OptionValueVariant>().ToListAsync(ct))
                .Select(ovv => (ovv.VariantId, ovv.OptionValueId)));

            foreach (var a in assignments)
            {
                var ov = optionValues.FirstOrDefault(v =>
                    v.Name.Equals(a.OptionValueName, StringComparison.OrdinalIgnoreCase) &&
                    v.OptionTypeId == Guid.Parse(a.OptionTypeId));
                if (ov is null) continue;

                if (existingAssignments.Contains((Guid.Parse(a.VariantId), ov.Id)))
                    continue;

                var assocResult = OptionValueVariantMethod.Create(
                    Guid.Parse(a.VariantId), ov.Id);
                if (assocResult.IsSuccess)
                    Context.Set<OptionValueVariant>().Add(assocResult.Value);
            }
            await Context.SaveChangesAsync(ct);
        }

        if (classifications is not null)
        {
            var existingClassifications = new HashSet<(Guid ProductId, Guid TaxonId)>(
                (await Context.Set<Classification>().ToListAsync(ct))
                .Where(c => c.ProductId.HasValue && c.TaxonId.HasValue)
                .Select(c => (c.ProductId!.Value, c.TaxonId!.Value)));

            foreach (var c in classifications)
            {
                if (existingClassifications.Contains((Guid.Parse(c.ProductId), Guid.Parse(c.TaxonId))))
                    continue;

                var result = ClassificationMethod.Create(
                    Guid.Parse(c.ProductId), Guid.Parse(c.TaxonId),
                    c.Position, isAutomatic: true);
                if (result.IsSuccess)
                    Context.Set<Classification>().Add(result.Value);
            }
            await Context.SaveChangesAsync(ct);
        }
    }

    private void AddProductOptionType(Guid productId, Guid optionTypeId, int position)
    {
        var result = ProductOptionTypeMethod.Create(productId, optionTypeId, position);
        if (result.IsSuccess)
            Context.Set<ProductOptionType>().Add(result.Value);
    }

    private record DemoProductJson
    {
        public string Id { get; init; } = default!;
        public string Name { get; init; } = default!;
        public string Slug { get; init; } = default!;
        public string Description { get; init; } = default!;
        public string Status { get; init; } = default!;
        public string GenderTarget { get; init; } = default!;
        public string MetaTitle { get; init; } = default!;
        public string MetaKeywords { get; init; } = default!;
        public string MasterVariantId { get; init; } = default!;
        public string? StyleCode { get; init; }
        public string? SeasonName { get; init; }
        public string? MaterialComposition { get; init; }
        public string? CareInstructions { get; init; }
        public string? Department { get; init; }
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
    }
    private record DemoVariantImageJson
    {
        public string Id { get; init; } = default!;
        public string VariantId { get; init; } = default!;
        public string ContentType { get; init; } = default!;
        public string FileName { get; init; } = default!;
        public string StoragePath { get; init; } = default!;
        public int Position { get; init; }
        public string Alt { get; init; } = default!;
        public string Type { get; init; } = default!;
    }
    private record DemoOptionAssignmentJson
    {
        public string VariantId { get; init; } = default!;
        public string OptionValueName { get; init; } = default!;
        public string OptionTypeId { get; init; } = default!;
    }
    private record DemoClassificationJson
    {
        public string ProductId { get; init; } = default!;
        public string TaxonId { get; init; } = default!;
        public int Position { get; init; }
    }
}
