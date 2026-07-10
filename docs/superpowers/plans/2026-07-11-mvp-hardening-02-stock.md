# Plan 2: Stock Integrity — Atomic Operations & Soft Reservation Fix

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix stock inflation bug on reservation release and make all stock mutations atomic to prevent race conditions.

**Architecture:** Remove `CountOnHand` increments from release handlers (soft reservation model: reserve never decrements, so release must never increment). Replace TOCTOU check-then-act patterns with `ExecuteUpdateAsync` atomic arithmetic.

**Tech Stack:** .NET 10, EF Core, PostgreSQL

## Global Constraints

- Stock reservations are "soft" — `CountOnHand` is NOT decremented on reserve. Available = `CountOnHand - SUM(active reservations)`.
- `ExecuteUpdateAsync` is the preferred atomic operation pattern (available since EF Core 7).
- `TreatWarningsAsErrors=true` globally.

---

## File Structure

| Action | File | Responsibility |
|--------|------|---------------|
| Modify | `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Release/ReleaseCartReservation.cs` | Remove `CountOnHand` increment |
| Modify | `service/Api/src/Module/Inventory/Features/Admin/StockReservations/Cancel/CancelStockReservation.cs` | Remove `CountOnHand` increment |
| Modify | `service/Api/src/Module/Inventory/Features/Admin/StockTransfers/Transfer/TransferStockTransfer.cs` | Atomic check-and-decrement |
| Modify | `service/Api/src/Module/Inventory/Features/Admin/StockItems/BulkAdjust/BulkAdjustStockItems.cs` | Atomic arithmetic |
| Modify | `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Reserve/ReserveCartStock.cs` | Serializable transaction |
| Modify | `service/Api/src/Module/Inventory/Features/Admin/StockTransfers/Cancel/CancelStockTransfer.cs` | Add StockMovement audit record |
| Modify | `service/Api/src/Module/Inventory/Features/Admin/StockTransfers/Receive/ReceiveStockTransfer.cs` | Error on missing destination StockItem |
| Modify | `service/Api/src/Module/Inventory/Features/Admin/StockItems/Restock/RestockStockItem.cs` | Fix audit trail + inject ICurrentUser/ILogger |

---

### Task 1: Fix ReleaseCartReservation — Remove Stock Inflation

**Files:**
- Modify: `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Release/ReleaseCartReservation.cs`

**Interfaces:**
- Consumes: `StockReservation` entity, `StockItem` entity

- [ ] **Step 1: Read the current handler**

Read `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Release/ReleaseCartReservation.cs` — find the block that increments `CountOnHand`.

- [ ] **Step 2: Remove the CountOnHand increment**

Delete the entire block that does `stockItem.CountOnHand += reservation.Quantity;`. The handler should only:
1. Find the reservation
2. Set `reservation.State = ReservationState.Released`
3. Set `reservation.ModifiedAtUtc = DateTimeOffset.UtcNow`
4. Save changes

The corrected handler body (after finding the reservation):
```csharp
reservation.State = ReservationState.Released;
reservation.ModifiedAtUtc = DateTimeOffset.UtcNow;

await dbContext.SaveChangesAsync(cancellationToken);

return reservation.MapToDetail<Response>();
```

Remove the `if (reservation.StockLocationId.HasValue)` block entirely — it was incrementing `CountOnHand` which was never decremented during reserve.

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Inventory/Module.Inventory.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Release/ReleaseCartReservation.cs
git commit -m "fix(inventory): remove CountOnHand increment on reservation release

Stock reservations are soft — CountOnHand is never decremented during reserve.
The release was incorrectly incrementing CountOnHand, causing stock to inflate
unboundedly over time."
```

---

### Task 2: Fix CancelStockReservation — Remove Stock Inflation

**Files:**
- Modify: `service/Api/src/Module/Inventory/Features/Admin/StockReservations/Cancel/CancelStockReservation.cs`

**Interfaces:**
- Consumes: Same entities as Task 1

- [ ] **Step 1: Read the current handler**

Read `service/Api/src/Module/Inventory/Features/Admin/StockReservations/Cancel/CancelStockReservation.cs` — find the stock restoration block.

- [ ] **Step 2: Remove the CountOnHand increment**

Delete the entire `if (reservation.StockLocationId.HasValue)` block that does `stockItem.CountOnHand += reservation.Quantity;`. The handler should only:
1. Find the reservation
2. Set `reservation.State = ReservationState.Released`
3. Set `reservation.ModifiedAtUtc = DateTimeOffset.UtcNow`
4. Save changes

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Inventory/Module.Inventory.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/Admin/StockReservations/Cancel/CancelStockReservation.cs
git commit -m "fix(inventory): remove CountOnHand increment on admin reservation cancel"
```

---

### Task 3: Make BulkAdjustStockItems Atomic

**Files:**
- Modify: `service/Api/src/Module/Inventory/Features/Admin/StockItems/BulkAdjust/BulkAdjustStockItems.cs`

**Interfaces:**
- Consumes: `StockItem` entity with `CountOnHand` property

- [ ] **Step 1: Read the current handler**

Read `service/Api/src/Module/Inventory/Features/Admin/StockItems/BulkAdjust/BulkAdjustStockItems.cs` — find the loop that loads entities, adjusts, and saves.

- [ ] **Step 2: Replace with ExecuteUpdateAsync**

Replace the load-adjust-save loop with atomic operations:

```csharp
foreach (var item in command.Request.Items)
{
    var affected = await dbContext.Set<StockItem>()
        .Where(x => x.Id == item.StockItemId)
        .ExecuteUpdateAsync(s => s
            .SetProperty(x => x.CountOnHand, x => x.CountOnHand + item.Quantity),
        cancellationToken);

    if (affected == 0)
        return StockItemResult.Errors.NotFound(item.StockItemId);
}
```

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Inventory/Module.Inventory.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/Admin/StockItems/BulkAdjust/BulkAdjustStockItems.cs
git commit -m "fix(inventory): use atomic ExecuteUpdateAsync for bulk stock adjustment

Prevents race condition where concurrent adjustments overwrite each other."
```

---

### Task 4: Make TransferStockTransfer Atomic

**Files:**
- Modify: `service/Api/src/Module/Inventory/Features/Admin/StockTransfers/Transfer/TransferStockTransfer.cs`

**Interfaces:**
- Consumes: `StockItem` entity, `StockTransfer` entity

- [ ] **Step 1: Read the current handler**

Read `service/Api/src/Module/Inventory/Features/Admin/StockTransfers/Transfer/TransferStockTransfer.cs` — find the two-query pattern (check availability, then decrement).

- [ ] **Step 2: Replace with atomic check-and-decrement**

Replace the two separate queries with a single atomic operation per transfer item:

```csharp
foreach (var item in entity.TransferItems)
{
    var affected = await dbContext.Set<StockItem>()
        .Where(x => x.VariantId == item.VariantId
            && x.StockLocationId == entity.SourceStockLocationId
            && x.CountOnHand >= item.Quantity)
        .ExecuteUpdateAsync(s => s
            .SetProperty(x => x.CountOnHand, x => x.CountOnHand - item.Quantity),
        cancellationToken);

    if (affected == 0)
        return StockTransferResult.Errors.InsufficientStock(item.VariantId);
}
```

Remove the earlier `FirstOrDefaultAsync` query that was used for the availability check — it's now redundant.

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Inventory/Module.Inventory.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/Admin/StockTransfers/Transfer/TransferStockTransfer.cs
git commit -m "fix(inventory): atomic check-and-decrement for stock transfer dispatch

Prevents race condition where concurrent transfers from same source
both pass availability check and both decrement, driving stock negative."
```

---

### Task 5: Make ReserveCartStock Atomic

**Files:**
- Modify: `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Reserve/ReserveCartStock.cs`

**Interfaces:**
- Consumes: `StockItem`, `StockReservation` entities

- [ ] **Step 1: Read the current handler**

Read `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Reserve/ReserveCartStock.cs` — find the availability check + insert pattern.

- [ ] **Step 2: Wrap in a serializable transaction**

Use `DatabaseFacade.BeginTransactionAsync(IsolationLevel.Serializable)`:

```csharp
await using var transaction = await dbContext.Database.BeginTransactionAsync(
    IsolationLevel.Serializable, cancellationToken);

try
{
    // Lock the stock row
    var stockItem = await dbContext.Set<StockItem>()
        .FromSqlRaw("SELECT * FROM inventory.stock_items WHERE variant_id = {0} AND stock_location_id = {1} FOR UPDATE",
            variantId, stockLocationId)
        .FirstOrDefaultAsync(cancellationToken);

    if (stockItem is null)
    {
        await transaction.RollbackAsync(cancellationToken);
        return StockReservationResult.Errors.InsufficientStock;
    }

    var reserved = await dbContext.Set<StockReservation>()
        .Where(r => r.VariantId == variantId
                    && r.StockLocationId == stockLocationId
                    && r.State == ReservationState.Reserved
                    && r.ExpiresAtUtc > DateTimeOffset.UtcNow)
        .SumAsync(r => r.Quantity, cancellationToken);

    var available = stockItem.CountOnHand - reserved;
    if (available < quantity)
    {
        await transaction.RollbackAsync(cancellationToken);
        return StockReservationResult.Errors.InsufficientStock;
    }

    var result = StockReservationMethod.Reserve(variantId, quantity, stockLocationId, null, ttlMinutes, cartToken: cartToken);
    if (result.IsFailure)
    {
        await transaction.RollbackAsync(cancellationToken);
        return result.Errors;
    }

    dbContext.Set<StockReservation>().Add(result.Value);
    await dbContext.SaveChangesAsync(cancellationToken);
    await transaction.CommitAsync(cancellationToken);

    return new Response
    {
        Id = result.Value.Id,
        VariantId = result.Value.VariantId,
        Quantity = result.Value.Quantity,
        ExpiresAtUtc = result.Value.ExpiresAtUtc!.Value,
        State = result.Value.State.ToString()
    };
}
catch
{
    await transaction.RollbackAsync(cancellationToken);
    throw;
}
```

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Inventory/Module.Inventory.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Reserve/ReserveCartStock.cs
git commit -m "fix(inventory): serializable transaction for stock reservation

Prevents over-reservation race condition where concurrent requests
both see sufficient availability and both insert reservations."
```

---

### Task 6: Add StockMovement Audit to CancelStockTransfer

**Files:**
- Modify: `service/Api/src/Module/Inventory/Features/Admin/StockTransfers/Cancel/CancelStockTransfer.cs`

**Interfaces:**
- Consumes: `StockMovement` entity, `StockMovementMethod.Create()` (or equivalent)

- [ ] **Step 1: Read the current handler**

Read `service/Api/src/Module/Inventory/Features/Admin/StockTransfers/Cancel/CancelStockTransfer.cs` — find the `if (wasInTransit)` block that restores stock.

- [ ] **Step 2: Add StockMovement creation inside the restore loop**

After `stockItem.CountOnHand += item.Quantity;` (or after the atomic restore), add:

```csharp
var movement = StockMovementMethod.Create(
    stockItemId: stockItem.Id,
    action: "transfer_canceled",
    quantity: item.Quantity,
    previousCountOnHand: stockItem.CountOnHand - item.Quantity,
    referenceId: entity.Id);

if (movement.IsSuccess)
    dbContext.Set<StockMovement>().Add(movement.Value);
```

Add the using if needed:
```csharp
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
```

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Inventory/Module.Inventory.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/Admin/StockTransfers/Cancel/CancelStockTransfer.cs
git commit -m "fix(inventory): add StockMovement audit when canceling stock transfer"
```

---

### Task 7: Fix ReceiveStockTransfer — Error on Missing Destination

**Files:**
- Modify: `service/Api/src/Module/Inventory/Features/Admin/StockTransfers/Receive/ReceiveStockTransfer.cs`

**Interfaces:**
- Consumes: `StockItem` entity, `StockTransferResult.Errors`

- [ ] **Step 1: Read the current handler**

Read `service/Api/src/Module/Inventory/Features/Admin/StockTransfers/Receive/ReceiveStockTransfer.cs` — find where `destStockItem is not null` silently skips.

- [ ] **Step 2: Replace silent skip with error return**

Change:
```csharp
if (destStockItem is not null)
{
    // ... add stock
}
```

To:
```csharp
if (destStockItem is null)
    return StockTransferResult.Errors.DestinationStockItemNotFound(item.VariantId);

// ... add stock
```

If `StockTransferResult.Errors.DestinationStockItemNotFound` doesn't exist, add it to the result class:
```csharp
public static Error DestinationStockItemNotFound(Guid variantId) =>
    Error.NotFound("StockTransfer.DestinationStockItem.NotFound",
        $"Destination stock item for variant {variantId} was not found at the destination location.");
```

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Inventory/Module.Inventory.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/Admin/StockTransfers/Receive/ReceiveStockTransfer.cs
git commit -m "fix(inventory): error on missing destination stock item during receive

Previously silently skipped, leaving transfer marked received while
physical inventory at destination was short."
```

---

### Task 8: Fix RestockStockItem — Audit Trail + DI

**Files:**
- Modify: `service/Api/src/Module/Inventory/Features/Admin/StockItems/Restock/RestockStockItem.cs`

**Interfaces:**
- Consumes: `ICurrentUser`, `ILogger<CommandHandler>`, `StockMovementMethod`

- [ ] **Step 1: Read the current handler**

Read `service/Api/src/Module/Inventory/Features/Admin/StockItems/Restock/RestockStockItem.cs`.

- [ ] **Step 2: Add ICurrentUser and ILogger to constructor**

Change constructor from:
```csharp
public sealed class CommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<Command, Response>
```

To:
```csharp
public sealed class CommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    ILogger<CommandHandler> logger)
    : ICommandHandler<Command, Response>
```

- [ ] **Step 3: Fix previousCount capture timing**

Move `var previousCount = stockItem.CountOnHand;` to AFTER the backorder fulfillment loop but BEFORE the remaining stock addition. The line should be just before `stockItem.CountOnHand += remaining;`.

- [ ] **Step 4: Fix movement creation failure handling**

Change:
```csharp
if (restockMovement.IsSuccess)
    movementId = restockMovement.Value.Id;
else
    movementId = Guid.Empty;
```

To:
```csharp
if (restockMovement.IsFailure)
    return restockMovement.Errors;

movementId = restockMovement.Value.Id;
```

- [ ] **Step 5: Set audit fields**

Before `await dbContext.SaveChangesAsync(cancellationToken);`, add:
```csharp
stockItem.ModifiedBy = currentUser.UserName;
```

- [ ] **Step 6: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Inventory/Module.Inventory.csproj`
Expected: Build succeeds

- [ ] **Step 7: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/Admin/StockItems/Restock/RestockStockItem.cs
git commit -m "fix(inventory): fix restock audit trail and add DI dependencies

- Capture previousCount after backorder loop for accurate audit
- Return error when movement creation fails (not Guid.Empty)
- Inject ICurrentUser for audit fields, ILogger for observability"
```

---

### Task 9: Build and Verify All Changes

- [ ] **Step 1: Full solution build**

Run: `dotnet build`
Expected: Build succeeds with zero warnings

- [ ] **Step 2: Run unit tests**

Run: `dotnet test service/Api/tests/Module.UnitTests`
Expected: All tests pass

- [ ] **Step 3: Commit (if any fixes needed)**

```bash
git commit -m "fix: address build warnings from stock integrity fixes"
```
