using Module.Inventory.Features.Admin.StockItems.Shared.Models;

namespace Module.Inventory.Features.Admin.StockItems.Shared.Mappings;

public static class ImportStockItemsMapping
{
    public static T MapToImportResult<T>(
        this (int Created, int Updated, List<string> Errors) source)
        where T : ImportStockItemsResponseBase, new()
        => new T
        {
            Created = source.Created,
            Updated = source.Updated,
            Failed = source.Errors.Count,
            Errors = source.Errors
        };
}
