using Module.Inventory.Domain.StockLocations.StockItems;

namespace Module.Inventory.Features.Admin.StockItems.Import;

/// <summary>Handles bulk import of stock items from CSV file.</summary>
public static partial class ImportStockItems
{
    public sealed record Command(IFormFile File) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Executes the import stock items command.</summary>
        /// <param name="command">The command containing the CSV file.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result containing import statistics and errors.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            var response = new Response();

            // Parse: Read CSV file
            using var reader = new StreamReader(command.File.OpenReadStream());
            var headerLine = await reader.ReadLineAsync(cancellationToken);
            if (headerLine is null)
                return response;

            var headers = headerLine.Split(',');
            var variantIdIdx = Array.FindIndex(headers, h => h.Trim().Equals("variant_id", StringComparison.OrdinalIgnoreCase));
            var locationIdIdx = Array.FindIndex(headers, h => h.Trim().Equals("stock_location_id", StringComparison.OrdinalIgnoreCase));
            var countIdx = Array.FindIndex(headers, h => h.Trim().Equals("count_on_hand", StringComparison.OrdinalIgnoreCase));
            var backorderIdx = Array.FindIndex(headers, h => h.Trim().Equals("backorderable", StringComparison.OrdinalIgnoreCase));

            if (variantIdIdx < 0 || locationIdIdx < 0 || countIdx < 0)
            {
                response.Errors.Add(new ImportError { Row = 0, Error = "CSV must have columns: variant_id, stock_location_id, count_on_hand" });
                return response;
            }

            var row = 1;
            string? line;
            while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
            {
                row++;
                if (string.IsNullOrWhiteSpace(line)) continue;

                try
                {
                    var values = line.Split(',');
                    if (values.Length <= Math.Max(variantIdIdx, Math.Max(locationIdIdx, countIdx)))
                    {
                        response.Errors.Add(new ImportError { Row = row, Error = "Invalid row format" });
                        continue;
                    }

                    if (!Guid.TryParse(values[variantIdIdx].Trim(), out var variantId))
                    {
                        response.Errors.Add(new ImportError { Row = row, VariantId = values[variantIdIdx], Error = "Invalid variant_id" });
                        continue;
                    }

                    if (!Guid.TryParse(values[locationIdIdx].Trim(), out var locationId))
                    {
                        response.Errors.Add(new ImportError { Row = row, Error = "Invalid stock_location_id" });
                        continue;
                    }

                    if (!int.TryParse(values[countIdx].Trim(), out var countOnHand) || countOnHand < 0)
                    {
                        response.Errors.Add(new ImportError { Row = row, VariantId = values[variantIdIdx], Error = "Invalid count_on_hand (must be >= 0)" });
                        continue;
                    }

                    var backorderable = false;
                    if (backorderIdx >= 0 && backorderIdx < values.Length)
                        _ = bool.TryParse(values[backorderIdx].Trim(), out backorderable);

                    var existing = await dbContext.Set<StockItem>()
                        .FirstOrDefaultAsync(si => si.VariantId == variantId && si.StockLocationId == locationId, cancellationToken);

                    if (existing is not null)
                    {
                        existing.CountOnHand = countOnHand;
                        existing.Backorderable = backorderable;
                        existing.ModifiedAtUtc = DateTimeOffset.UtcNow;
                    }
                    else
                    {
                        var createResult = StockItemMethod.Create(locationId, variantId, backorderable, countOnHand);
                        if (createResult.IsFailure)
                        {
                            response.Errors.Add(new ImportError { Row = row, VariantId = values[variantIdIdx], Error = "Failed to create stock item" });
                            continue;
                        }

                        dbContext.Set<StockItem>().Add(createResult.Value);
                    }

                    response.Imported++;
                }
                catch (Exception ex)
                {
                    response.Errors.Add(new ImportError { Row = row, Error = ex.Message });
                }
            }

            // Persist: Save all imported items
            if (response.Imported > 0)
                await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return import results
            return response;
        }
    }
}
