using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Features.Storefront.Products.Shared.Models;

namespace Module.Catalog.Features.Storefront.Products.Get.Availability;

/// <summary>
/// Defines the use case for retrieving product availability matrix.
/// </summary>
public static partial class GetAvailability
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    /// <summary>
    /// Retrieves the style matrix availability grid for a product,
    /// computing per-variant stock status grouped by OptionType axes (Color x Size).
    /// </summary>
    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>
        /// Handles the availability query — loads non-master variants with option values
        /// and prices, computes availability status, and groups into a matrix response.
        /// </summary>
        /// <param name="query">The query containing the product ID.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A success result with the availability matrix.</returns>
        // Contract: pre=query.Id!=Guid.Empty, post=result!=null
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var productExists = await dbContext.Set<Product>()
                .AnyAsync(x => x.Id == query.Id && !x.IsDeleted, cancellationToken);
            if (!productExists)
                return Result<Response>.NotFound();

            var variants = await dbContext.Set<Variant>()
                .Include(v => v.OptionValueVariants)
                    .ThenInclude(ov => ov.OptionValue!)
                        .ThenInclude(o => o.OptionType!)

                .Include(v => v.Prices)
                .Where(v => v.ProductId == query.Id && !v.IsDeleted && !v.IsMaster)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var optionTypes = variants
                .SelectMany(v => v.OptionValueVariants)
                .Select(ov => ov.OptionValue?.OptionType)
                .Where(ot => ot != null)
                .Distinct()
                .OrderBy(ot => ot!.Position)
                .ToList();

            var axes = optionTypes.Select(ot => new AvailabilityAxis
            {
                Name = ot!.Name,
                Presentation = ot.Presentation,
                Values = variants
                    .SelectMany(v => v.OptionValueVariants)
                    .Where(ov => ov.OptionValue?.OptionTypeId == ot.Id)
                    .Select(ov => ov.OptionValue!)
                    .Distinct()
                    .OrderBy(v => v.Position)
                    .Select(v => new AvailabilityAxisValue
                    {
                        Id = v.Id,
                        Name = v.Name,
                        Presentation = v.Presentation,
                    })
                    .ToList(),
            }).ToList();

            var cells = variants.Select(v =>
            {
                var ovs = v.OptionValueVariants
                    .OrderBy(ov => ov.OptionValue?.OptionType?.Position)
                    .ToList();

                var firstPrice = v.Prices.FirstOrDefault();

                return new AvailabilityCell
                {
                    VariantId = v.Id,
                    OptionValue1Id = ovs.Count > 0 ? ovs[0].OptionValueId : Guid.Empty,
                    OptionValue2Id = ovs.Count > 1 ? ovs[1].OptionValueId : null,
                    Status = firstPrice?.Amount > 0 ? "in_stock" : "unknown",
                    Price = firstPrice?.Amount,
                    Currency = firstPrice?.Currency,
                };
            }).ToList();

            return Result<Response>.Ok(new Response
            {
                Axes = axes,
                Cells = cells,
            });
        }
    }
}
