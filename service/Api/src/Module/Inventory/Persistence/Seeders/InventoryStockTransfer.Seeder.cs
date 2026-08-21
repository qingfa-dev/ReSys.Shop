using Module.Catalog.Domain.Variants;
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockTransfers;

namespace Module.Inventory.Persistence.Seeders;

public sealed class InventoryStockTransferSeeder(IApplicationDbContext context, DemoJsonHelper jsonHelper) : AbstractDataSeeder(context)
{
    public override int Order => 160;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        if (await HasDataAsync<StockTransfer>(cancellationToken))
            return Result.Ok();

        var locations = await Context.Set<StockLocation>().ToListAsync(cancellationToken);
        if (locations.Count < 2)
            return Result.Ok();

        var variants = await Context.Set<Variant>().Where(v => !v.IsDeleted).ToListAsync(cancellationToken);
        if (variants.Count == 0)
            return Result.Ok();

        var json = jsonHelper.LoadIfExists<DemoStockTransferJson>("012_demo_stock_transfers.json");
        if (json is not null && json.Length > 0)
        {
            foreach (var entry in json)
            {
                var source = locations.FirstOrDefault(l => l.Code == entry.SourceLocationCode);
                var dest = locations.FirstOrDefault(l => l.Code == entry.DestinationLocationCode);
                if (source is null || dest is null) continue;
                if (source.Id == dest.Id) continue;

                var items = new List<(Guid VariantId, int Quantity)>();
                foreach (var it in entry.Items)
                {
                    if (!Guid.TryParse(it.VariantId, out var vid)) continue;
                    if (!variants.Any(v => v.Id == vid)) continue;
                    if (it.Quantity <= 0) continue;
                    items.Add((vid, it.Quantity));
                }

                if (items.Count == 0) continue;

                var createResult = StockTransferExtensions.Create(entry.Reference, source.Id, dest.Id, items);
                if (createResult.IsFailure) continue;

                var transfer = createResult.Value;

                if (Guid.TryParse(entry.Id, out var parsedId))
                    transfer.Id = parsedId;

                if (!string.IsNullOrWhiteSpace(entry.Number))
                    transfer.Number = entry.Number;

                if (!string.IsNullOrWhiteSpace(entry.CreatedAtUtc) && DateTimeOffset.TryParse(entry.CreatedAtUtc, out var createdAt))
                    transfer.CreatedAtUtc = createdAt;

                // Fix child FK after possible Id override
                foreach (var ti in transfer.TransferItems)
                    ti.StockTransferId = transfer.Id;

                // Apply state transitions to match seed data
                if (!string.IsNullOrWhiteSpace(entry.State) && Enum.TryParse<TransferState>(entry.State, true, out var desiredState))
                {
                    ApplyDesiredState(transfer, desiredState, entry.Items);
                }

                Context.Set<StockTransfer>().Add(transfer);
            }

            await SaveChangesWithIdempotencyAsync(cancellationToken);
            return Result.Ok();
        }

        // Fallback: generate deterministic demo transfers without JSON
        var rng = new Random(42);
        var eligibleVariants = variants.Where(v => !v.IsMaster).ToList();
        if (eligibleVariants.Count == 0) eligibleVariants = variants;

        var pairs = new[]
        {
            (Src: "MAIN", Dst: "EAST"),
            (Src: "EAST", Dst: "EXPRESS"),
            (Src: "MAIN", Dst: "EXPRESS"),
            (Src: "EXPRESS", Dst: "MAIN"),
        };

        var states = new[] { TransferState.Draft, TransferState.InTransit, TransferState.Received, TransferState.Canceled };

        int transferCount = Math.Min(12, eligibleVariants.Count);
        for (int i = 0; i < transferCount; i++)
        {
            var pair = pairs[i % pairs.Length];
            var src = locations.FirstOrDefault(l => l.Code == pair.Src);
            var dst = locations.FirstOrDefault(l => l.Code == pair.Dst);
            if (src is null || dst is null) continue;

            int itemCount = rng.Next(1, 4);
            var picked = eligibleVariants.OrderBy(_ => rng.Next()).Take(itemCount).ToList();
            var items = picked.Select(v => (v.Id, Quantity: rng.Next(5, 30))).ToList();

            var result = StockTransferExtensions.Create($"SEED-REF-{i + 1:000}", src.Id, dst.Id, items);
            if (result.IsFailure) continue;

            var transfer = result.Value;
            // Deterministic number for demo readability
            transfer.Number = $"T20260522-{1000 + i}";

            var desiredState = states[i % states.Length];
            ApplyDesiredState(transfer, desiredState, null);

            Context.Set<StockTransfer>().Add(transfer);
        }

        await SaveChangesWithIdempotencyAsync(cancellationToken);
        return Result.Ok();
    }

    private static void ApplyDesiredState(StockTransfer transfer, TransferState desiredState, DemoTransferItemJson[]? jsonItems)
    {
        switch (desiredState)
        {
            case TransferState.Draft:
                break;

            case TransferState.InTransit:
                transfer.Transfer();
                break;

            case TransferState.Received:
                transfer.Transfer();
                // Fully receive all items
                foreach (var item in transfer.TransferItems.ToList())
                {
                    transfer.Receive(item.VariantId, item.Quantity);
                }
                // If json specifies partial received quantities, override after full receive logic
                if (jsonItems is not null)
                {
                    foreach (var ji in jsonItems)
                    {
                        if (!Guid.TryParse(ji.VariantId, out var vid)) continue;
                        var ti = transfer.TransferItems.FirstOrDefault(t => t.VariantId == vid);
                        if (ti is not null && ji.ReceivedQuantity >= 0 && ji.ReceivedQuantity <= ti.Quantity)
                            ti.ReceivedQuantity = ji.ReceivedQuantity;
                    }
                    // Re-evaluate state if not fully received
                    if (transfer.TransferItems.Any(t => t.ReceivedQuantity < t.Quantity))
                        transfer.State = TransferState.InTransit;
                }
                break;

            case TransferState.Canceled:
                // Cancel from Draft if possible, otherwise transfer then cancel
                var cancelResult = transfer.Cancel();
                if (cancelResult.IsFailure)
                {
                    transfer.Transfer();
                    transfer.Cancel();
                }
                break;
        }
    }

    private sealed record DemoStockTransferJson
    {
        public string Id { get; init; } = default!;
        public string? Number { get; init; }
        public string? Reference { get; init; }
        public string SourceLocationCode { get; init; } = default!;
        public string DestinationLocationCode { get; init; } = default!;
        public string State { get; init; } = "Draft";
        public string? CreatedAtUtc { get; init; }
        public DemoTransferItemJson[] Items { get; init; } = [];
    }

    private sealed record DemoTransferItemJson
    {
        public string VariantId { get; init; } = default!;
        public int Quantity { get; init; }
        public int ReceivedQuantity { get; init; }
    }
}
