# Plan 5: Inventory, Shipping & Location Fixes

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix inventory audit trails, import safety, shipping rate matching, and location validation.

**Architecture:** Fix stock movement audit in cancel/restock. Add file size limits to CSV import. Fix weight-bound matching in shipping calculator. Enforce ISO code format and case-insensitive uniqueness.

**Tech Stack:** .NET 10, EF Core, FluentValidation

## Global Constraints

- `TreatWarningsAsErrors=true` globally.
- All handlers return `Result<T>` or `Result`.

---

## File Structure

| Action | File | Responsibility |
|--------|------|---------------|
| Modify | `service/Api/src/Module/Inventory/Features/Admin/StockTransfers/Cancel/CancelStockTransfer.cs` | Add movement audit |
| Modify | `service/Api/src/Module/Inventory/Features/Admin/StockItems/Restock/RestockStockItem.cs` | Fix audit + DI |
| Modify | `service/Api/src/Module/Inventory/Features/Admin/StockItems/Import/ImportStockItems.cs` | Add file size limit |
| Modify | `service/Api/src/Module/Shipping/Domain/Calculators/ShippingRateCalculator.cs` | Fix null weight bounds |
| Modify | `service/Api/src/Module/Shipping/Features/Admin/ShippingMethods/Deactivate/DeactivateShippingMethod.cs` | Check active orders |
| Modify | `service/Api/src/Module/Shipping/Features/Admin/ShippingMethods/Delete/DeleteShippingMethod.cs` | Check rates + orders |
| Modify | `service/Api/src/Module/Shipping/Features/Storefront/Shipping/Calculate/CalculateShipping.cs` | Fix hardcoded currency |
| Modify | `service/Api/src/Module/Location/Features/Admin/Countries/Create/CreateCountry.cs` | Case-insensitive ISO check |
| Modify | `service/Api/src/Module/Location/Features/Admin/Countries/Shared/Validators/Country.Validator.IsoCode.cs` | Enforce ISO format |
| Modify | `service/Api/src/Module/Location/Features/Admin/States/Delete/DeleteState.cs` | Add referential check |

---

### Task 1: Fix CancelStockTransfer — Add Movement Audit (Stock Plan Task 6)

> This task was already defined in Plan 2, Task 6. If not yet implemented, execute it here.

- [ ] **Step 1: Verify Plan 2 Task 6 is complete**

Check if `CancelStockTransfer.cs` already has `StockMovement` creation. If yes, skip. If no, implement.

- [ ] **Step 2: Commit (if needed)**

```bash
git commit -m "fix(inventory): add StockMovement audit when canceling stock transfer"
```

---

### Task 2: Fix RestockStockItem — Audit + DI (Stock Plan Task 8)

> This task was already defined in Plan 2, Task 8. If not yet implemented, execute it here.

- [ ] **Step 1: Verify Plan 2 Task 8 is complete**

Check if `RestockStockItem.cs` already has `ICurrentUser` and `ILogger` injection. If yes, skip. If no, implement.

- [ ] **Step 2: Commit (if needed)**

```bash
git commit -m "fix(inventory): fix restock audit trail and add DI dependencies"
```

---

### Task 3: Add File Size Limit to CSV Import

**Files:**
- Modify: `service/Api/src/Module/Inventory/Features/Admin/StockItems/Import/ImportStockItems.cs`

**Interfaces:**
- Consumes: `IFormFile`

- [ ] **Step 1: Read the current handler**

Read `service/Api/src/Module/Inventory/Features/Admin/StockItems/Import/ImportStockItems.cs`.

- [ ] **Step 2: Add file size check**

After the `file.Length == 0` check, add:
```csharp
const long MaxFileSize = 5_242_880; // 5 MB
if (file.Length > MaxFileSize)
    return Error.Validation("StockItem.Import.FileTooLarge", "CSV file must not exceed 5 MB.");
```

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Inventory/Module.Inventory.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/Admin/StockItems/Import/ImportStockItems.cs
git commit -m "fix(inventory): add 5 MB file size limit to CSV stock import"
```

---

### Task 4: Fix ShippingRateCalculator Weight Bounds

**Files:**
- Modify: `service/Api/src/Module/Shipping/Domain/Calculators/ShippingRateCalculator.cs`

**Interfaces:**
- Consumes: `ShippingRate` entity with `MinWeight`/`MaxWeight`

- [ ] **Step 1: Read the current calculator**

Read `service/Api/src/Module/Shipping/Domain/Calculators/ShippingRateCalculator.cs`.

- [ ] **Step 2: Fix null bound handling**

Change:
```csharp
(r.MinWeight <= orderWeight && r.MaxWeight >= orderWeight)
```

To:
```csharp
(r.MinWeight == null || r.MinWeight <= orderWeight)
&& (r.MaxWeight == null || r.MaxWeight >= orderWeight)
```

This handles rates with only a floor (MinWeight set, MaxWeight null) or only a ceiling.

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Shipping/Module.Shipping.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Shipping/Domain/Calculators/ShippingRateCalculator.cs
git commit -m "fix(shipping): handle null MinWeight/MaxWeight in rate matching"
```

---

### Task 5: Add Active Order Check to DeactivateShippingMethod

**Files:**
- Modify: `service/Api/src/Module/Shipping/Features/Admin/ShippingMethods/Deactivate/DeactivateShippingMethod.cs`

**Interfaces:**
- Consumes: `Order` entity (from Ordering module via shared DbContext)

- [ ] **Step 1: Read the current handler**

Read `service/Api/src/Module/Shipping/Features/Admin/ShippingMethods/Deactivate/DeactivateShippingMethod.cs`.

- [ ] **Step 2: Add active order check**

Before setting `AvailableToUsers = false`, add:
```csharp
var hasActiveOrders = await dbContext.Set<Order>()
    .AnyAsync(o => o.ShippingMethodId == command.Id
        && o.Status != OrderStatus.Canceled
        && o.Status != OrderStatus.Completed,
    cancellationToken);

if (hasActiveOrders)
    return ShippingMethodResult.Failure.HasActiveOrders;
```

If the Shipping module doesn't have access to the `Order` entity, use a shared interface or skip this check (document the limitation).

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Shipping/Module.Shipping.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Shipping/Features/Admin/ShippingMethods/Deactivate/DeactivateShippingMethod.cs
git commit -m "fix(shipping): check for active orders before deactivating shipping method"
```

---

### Task 6: Add Referential Check to DeleteShippingMethod

**Files:**
- Modify: `service/Api/src/Module/Shipping/Features/Admin/ShippingMethods/Delete/DeleteShippingMethod.cs`

**Interfaces:**
- Consumes: `ShippingRate` entity

- [ ] **Step 1: Read the current handler**

Read `service/Api/src/Module/Shipping/Features/Admin/ShippingMethods/Delete/DeleteShippingMethod.cs`.

- [ ] **Step 2: Add rates check before deletion**

Before soft-deleting, add:
```csharp
var hasRates = await dbContext.Set<ShippingRate>()
    .AnyAsync(r => r.ShippingMethodId == command.Id, cancellationToken);

if (hasRates)
    return ShippingMethodResult.Failure.HasAssociatedRates;
```

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Shipping/Module.Shipping.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Shipping/Features/Admin/ShippingMethods/Delete/DeleteShippingMethod.cs
git commit -m "fix(shipping): check for associated rates before deleting shipping method"
```

---

### Task 7: Fix CalculateShipping Hardcoded Currency

**Files:**
- Modify: `service/Api/src/Module/Shipping/Features/Storefront/Shipping/Calculate/CalculateShipping.cs`

**Interfaces:**
- Consumes: Order or system config for currency

- [ ] **Step 1: Read the current handler**

Read `service/Api/src/Module/Shipping/Features/Storefront/Shipping/Calculate/CalculateShipping.cs`.

- [ ] **Step 2: Replace hardcoded currency**

Change `Currency = "USD"` to derive from order or config:
```csharp
Currency = order?.Currency ?? "USD"
```

Or if no order is available, accept it as a parameter or use system config.

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Shipping/Module.Shipping.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Shipping/Features/Storefront/Shipping/Calculate/CalculateShipping.cs
git commit -m "fix(shipping): derive currency from order instead of hardcoding USD"
```

---

### Task 8: Fix Country ISO Code — Case-Insensitive + Format

**Files:**
- Modify: `service/Api/src/Module/Location/Features/Admin/Countries/Create/CreateCountry.cs`
- Modify: `service/Api/src/Module/Location/Features/Admin/Countries/Shared/Validators/Country.Validator.IsoCode.cs`

**Interfaces:**
- N/A

- [ ] **Step 1: Read CreateCountry handler**

Read `service/Api/src/Module/Location/Features/Admin/Countries/Create/CreateCountry.cs`.

- [ ] **Step 2: Make duplicate check case-insensitive**

Change:
```csharp
c.IsoCode == request.IsoCode
```

To:
```csharp
c.IsoCode.ToUpper() == request.IsoCode.ToUpper()
```

- [ ] **Step 3: Read ISO code validator**

Read `service/Api/src/Module/Location/Features/Admin/Countries/Shared/Validators/Country.Validator.IsoCode.cs`.

- [ ] **Step 4: Add format validation**

Add `.Matches("^[A-Z]{2,3}$")` to the rule chain:
```csharp
RuleFor(x => x.IsoCode)
    .NotEmpty()
    .MaximumLength(3)
    .Matches("^[A-Z]{2,3}$").WithErrorCode("Country.IsoCode.InvalidFormat")
    .WithMessage("ISO code must be 2-3 uppercase letters.");
```

- [ ] **Step 5: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Location/Module.Location.csproj`
Expected: Build succeeds

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Location/Features/Admin/Countries/
git commit -m "fix(location): enforce ISO 3166 format and case-insensitive uniqueness"
```

---

### Task 9: Add Referential Check to DeleteState

**Files:**
- Modify: `service/Api/src/Module/Location/Features/Admin/States/Delete/DeleteState.cs`

**Interfaces:**
- Consumes: Address entity (if available)

- [ ] **Step 1: Read the current handler**

Read `service/Api/src/Module/Location/Features/Admin/States/Delete/DeleteState.cs`.

- [ ] **Step 2: Add referential integrity check**

Before hard-deleting, check for references:
```csharp
// If Address entity is accessible:
var hasAddresses = await dbContext.Set<Address>()
    .AnyAsync(a => a.StateProvinceId == command.Id, cancellationToken);

if (hasAddresses)
    return StateResult.Failure.HasAddresses;
```

If Address entity is not accessible from the Location module, change to soft-delete instead of hard-delete, or document the limitation.

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Location/Module.Location.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Location/Features/Admin/States/Delete/DeleteState.cs
git commit -m "fix(location): check referential integrity before deleting state"
```

---

### Task 10: Build and Verify All Changes

- [ ] **Step 1: Full solution build**

Run: `dotnet build`
Expected: Build succeeds with zero warnings

- [ ] **Step 2: Run unit tests**

Run: `dotnet test service/Api/tests/Module.UnitTests`
Expected: All tests pass

- [ ] **Step 3: Commit (if any fixes needed)**

```bash
git commit -m "fix: address build warnings from inventory/shipping/location fixes"
```
