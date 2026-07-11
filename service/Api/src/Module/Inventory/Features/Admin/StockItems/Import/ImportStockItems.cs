using System.Globalization;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Features.Admin.StockItems.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockItems.Import;

public static partial class ImportStockItems
{
    public sealed record Command(IFormFile File) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var file = command.File;
            if (file is null || file.Length == 0)
                return StockItemResult.Errors.ImportFileRequired;

            const long MaxFileSize = 5_242_880; // 5 MB
            if (file.Length > MaxFileSize)
                return Error.Validation("StockItem.Import.FileTooLarge", "CSV file must not exceed 5 MB.");

            using var reader = new StreamReader(file.OpenReadStream());
            var header = await reader.ReadLineAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(header))
                return StockItemResult.Errors.ImportEmptyFile;

            var errors = new List<string>();
            var created = 0;
            var updated = 0;
            var lineNumber = 1;
            string? line;

            while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
            {
                lineNumber++;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(',');
                if (parts.Length < 3)
                {
                    errors.Add($"Line {lineNumber}: Invalid format. Expected at least VariantId,StockLocationId,CountOnHand.");
                    continue;
                }

                try
                {
                    if (!Guid.TryParse(parts[0].Trim(), out var variantId))
                    {
                        errors.Add($"Line {lineNumber}: Invalid VariantId '{parts[0]}'.");
                        continue;
                    }

                    if (!Guid.TryParse(parts[1].Trim(), out var stockLocationId))
                    {
                        errors.Add($"Line {lineNumber}: Invalid StockLocationId '{parts[1]}'.");
                        continue;
                    }

                    if (!int.TryParse(parts[2].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var countOnHand) || countOnHand < 0)
                    {
                        errors.Add($"Line {lineNumber}: Invalid CountOnHand '{parts[2]}'.");
                        continue;
                    }

                    var backorderable = false;
                    if (parts.Length > 3 && !string.IsNullOrWhiteSpace(parts[3]))
                    {
                        if (!bool.TryParse(parts[3].Trim(), out backorderable))
                        {
                            errors.Add($"Line {lineNumber}: Invalid Backorderable '{parts[3]}'. Expected true or false.");
                            continue;
                        }
                    }

                    var existing = await dbContext.Set<StockItem>()
                        .FirstOrDefaultAsync(x => x.VariantId == variantId && x.StockLocationId == stockLocationId, cancellationToken);

                    if (existing is not null)
                    {
                        existing.CountOnHand = countOnHand;
                        existing.Backorderable = backorderable;
                        existing.ModifiedAtUtc = DateTimeOffset.UtcNow;
                        existing.ModifiedBy = currentUser.UserName;
                        updated++;
                    }
                    else
                    {
                        var result = StockItemMethod.Create(stockLocationId, variantId, backorderable, countOnHand);
                        if (result.IsFailure)
                        {
                            foreach (var error in result.Errors)
                                errors.Add($"Line {lineNumber}: {error.Message}");
                            continue;
                        }

                        result.Value.CreatedBy = currentUser.UserName;
                        dbContext.Set<StockItem>().Add(result.Value);
                        created++;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Line {lineNumber}: Unexpected error - {ex.Message}");
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogDebug(
                "[StockItem.Import]: Created {Created}, Updated {Updated}, Failed {Failed}",
                created, updated, errors.Count);

            return new Response
            {
                Created = created,
                Updated = updated,
                Failed = errors.Count,
                Errors = errors
            };
        }
    }
}
