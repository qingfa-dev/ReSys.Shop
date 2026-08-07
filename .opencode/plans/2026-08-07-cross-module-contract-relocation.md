# Cross-Module Contract Relocation — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move 15 cross-module MediatR contracts from `Shared/Application/Contracts/` into module feature folders, remove `Contracts/` folder layer, delete 2 dead Catalog queries.

**Architecture:** Each cross-module service becomes a feature folder following the `CreateOptionType` pattern (static partial class with Command/Query + Handler + Request/Response). Handlers already exist in module folders — we move the Command/Query/Response records alongside them and delete the Shared originals.

**Tech Stack:** .NET 10, C#, MediatR, FluentValidation, Carter, EF Core + PostgreSQL

## Global Constraints

- All 9 modules share one `Module.csproj` assembly — no project references change
- `TreatWarningsAsErrors=true` — any warning fails build
- Domain error factories in `Module.{X}/Domain/{Entity}/{Entity}.Result.cs` — use existing errors, add new ones only if needed
- `Result<T>` / `Result` patterns for all return types — no inline error strings
- Tests use xUnit + FluentAssertions + Moq + EF Core InMemory

---

## File Structure

### Files to CREATE (new)

| Module | Path | Content |
|---|---|---|
| Inventory | `Features/Storefront/ReserveCartStock/ReserveCartStock.Command.cs` | `ReserveCartStockCommand` + `ReserveLineItem` records |
| Inventory | `Features/Storefront/ReserveCartStock/ReserveCartStock.Response.cs` | `ReserveCartStockResponse` record |
| Inventory | `Features/Storefront/ConsumeCartStockReservations/ConsumeCartStockReservations.Command.cs` | `ConsumeCartStockReservationsCommand` record |
| Inventory | `Features/Storefront/ReleaseCartStockReservations/ReleaseCartStockReservations.Command.cs` | `ReleaseCartStockReservationsCommand` record |
| Inventory | `Features/Storefront/CheckVariantAvailability/CheckVariantAvailability.Query.cs` | `CheckVariantAvailabilityQuery` record |
| Inventory | `Features/Storefront/CheckVariantAvailability/CheckVariantAvailability.Response.cs` | `CheckVariantAvailabilityResponse` record |
| Ordering | `Features/Storefront/GetCartForCheckout/GetCartForCheckout.Query.cs` | `GetCartForCheckoutQuery` record |
| Ordering | `Features/Storefront/GetCartForCheckout/GetCartForCheckout.Response.cs` | `GetCartForCheckoutResponse` + `CartLineItem` records |
| Ordering | `Features/Storefront/AdvanceCheckoutState/AdvanceCheckoutState.Command.cs` | `AdvanceCheckoutStateCommand` record |
| Ordering | `Features/Storefront/GetCartForShipping/GetCartForShipping.Query.cs` | `GetCartForShippingQuery` record |
| Ordering | `Features/Storefront/GetCartForShipping/GetCartForShipping.Response.cs` | `CartForShippingResponse` record |
| Payment | `Features/Storefront/GetPaymentForCheckout/GetPaymentForCheckout.Query.cs` | `GetPaymentForCheckoutQuery` record |
| Payment | `Features/Storefront/GetPaymentForCheckout/GetPaymentForCheckout.Response.cs` | `PaymentForCheckoutResponse` record |
| Payment | `Features/Storefront/MarkPaymentPaid/MarkPaymentPaid.Command.cs` | `MarkPaymentPaidCommand` record |
| Profile | `Features/Storefront/Profiles/Create/CreateProfile.Command.cs` | `CreateUserProfileCommand` + `CreateUserProfileResult` records |

### Files to MOVE (handler + delete old location)

| FROM | TO |
|---|---|
| `Inventory/.../Contracts/ReserveCartStock/ReserveCartStock.cs` | `Inventory/.../ReserveCartStock/ReserveCartStock.cs` |
| `Inventory/.../Contracts/ConsumeCartStockReservations/ConsumeCartStockReservations.cs` | `Inventory/.../ConsumeCartStockReservations/ConsumeCartStockReservations.cs` |
| `Inventory/.../Contracts/ReleaseCartStockReservations/ReleaseCartStockReservations.cs` | `Inventory/.../ReleaseCartStockReservations/ReleaseCartStockReservations.cs` |
| `Inventory/.../Contracts/CheckVariantAvailability/CheckVariantAvailability.cs` | `Inventory/.../CheckVariantAvailability/CheckVariantAvailability.cs` |
| `Ordering/.../Contracts/GetCartForCheckout/GetCartForCheckout.cs` | `Ordering/.../GetCartForCheckout/GetCartForCheckout.cs` |
| `Ordering/.../Contracts/AdvanceCheckoutState/AdvanceCheckoutState.cs` | `Ordering/.../AdvanceCheckoutState/AdvanceCheckoutState.cs` |
| `Ordering/.../Contracts/GetCartForShipping/GetCartForShipping.cs` | `Ordering/.../GetCartForShipping/GetCartForShipping.cs` |
| `Payment/.../Contracts/GetPaymentForCheckout/GetPaymentForCheckout.cs` | `Payment/.../GetPaymentForCheckout/GetPaymentForCheckout.cs` |
| `Payment/.../Contracts/MarkPaymentPaid/MarkPaymentPaid.cs` | `Payment/.../MarkPaymentPaid/MarkPaymentPaid.cs` |
| `Shared/.../Contracts/Inventory/IStockQuantityService.cs` | `Inventory/Services/Abstractions/IStockQuantityService.cs` |

### Files to UPDATE (consumer using statements)

| Module | File |
|---|---|
| Ordering | `Cart/Checkout/CreateOrderFromCart.cs` |
| Ordering | `Cart/AddItem/AddToCart.cs` |
| Ordering | `Cart/UpdateItemQuantity/UpdateCartItemQuantity.cs` |
| Ordering | `GetCartForShipping/GetCartForShipping.cs` |
| Payment | `Payment/CreateIntent/CreatePaymentIntent.cs` |
| Profile | `Addresses/Validators/Address.Validator.cs` |
| Identity | `Emails/Confirm/ConfirmEmail.cs` |
| Identity | `Auth/Login/External/Authenticate/ExternalAuthenticate.cs` |
| Inventory | 4 handler files (self-cleanup) |
| Ordering | 3 handler files (self-cleanup) |
| Payment | 2 handler files (self-cleanup) |
| Profile | 1 handler file (self-cleanup) |
| Location | 2 handler files (self-cleanup) |

### Files to DELETE

| File | Reason |
|---|---|
| `Shared/.../Contracts/Inventory/ConsumeCartStockReservationsCommand.cs` | Moved to Inventory module |
| `Shared/.../Contracts/Inventory/ReserveCartStockCommand.cs` | Moved to Inventory module |
| `Shared/.../Contracts/Inventory/ReleaseCartStockReservationsCommand.cs` | Moved to Inventory module |
| `Shared/.../Contracts/Inventory/CheckVariantAvailabilityQuery.cs` | Moved to Inventory module |
| `Shared/.../Contracts/Inventory/IStockQuantityService.cs` | Moved to Inventory module |
| `Shared/.../Contracts/Ordering/GetCartForCheckoutQuery.cs` | Moved to Ordering module |
| `Shared/.../Contracts/Ordering/AdvanceCheckoutStateCommand.cs` | Moved to Ordering module |
| `Shared/.../Contracts/Ordering/GetCartForShippingQuery.cs` | Moved to Ordering module |
| `Shared/.../Contracts/Payment/GetPaymentForCheckoutQuery.cs` | Moved to Payment module |
| `Shared/.../Contracts/Payment/MarkPaymentPaidCommand.cs` | Moved to Payment module |
| `Shared/.../Contracts/Profile/CreateUserProfileCommand.cs` | Moved to Profile module |
| `Shared/.../Contracts/Location/CountryExistsByIsoQuery.cs` | Moved into handler file |
| `Shared/.../Contracts/Location/StateExistsByIsoQuery.cs` | Moved into handler file |
| `Shared/.../Contracts/Catalog/GetVariantDiscontinuedStatusesQuery.cs` | Dead code — no handler |
| `Shared/.../Contracts/Catalog/GetVariantWeightsQuery.cs` | Dead code — no handler |
| `Shared/.../Contracts/Inventory/` (directory) | Empty after moves |
| `Shared/.../Contracts/Ordering/` (directory) | Empty after moves |
| `Shared/.../Contracts/Payment/` (directory) | Empty after moves |
| `Shared/.../Contracts/Profile/` (directory) | Empty after moves |
| `Shared/.../Contracts/Location/` (directory) | Empty after moves |
| `Shared/.../Contracts/Catalog/` (directory) | Empty after moves |

---

## Task 1: Inventory — ReserveCartStock

**Files:**
- Create: `Module/Inventory/Features/Storefront/ReserveCartStock/ReserveCartStock.Command.cs`
- Create: `Module/Inventory/Features/Storefront/ReserveCartStock/ReserveCartStock.Response.cs`
- Move: `Module/Inventory/Features/Storefront/Contracts/ReserveCartStock/ReserveCartStock.cs` → `Module/Inventory/Features/Storefront/ReserveCartStock/ReserveCartStock.cs`
- Delete: `Shared/Application/Contracts/Inventory/ReserveCartStockCommand.cs`

- [ ] **Step 1: Create command file**

Create `Module/Inventory/Features/Storefront/ReserveCartStock/ReserveCartStock.Command.cs`:

```csharp
namespace Module.Inventory.Features.Storefront.Contracts.ReserveCartStock;

public sealed record ReserveCartStockCommand : ICommand<ReserveCartStockResponse>
{
    public Guid CartId { get; init; }
    public IReadOnlyList<ReserveLineItem> LineItems { get; init; } = [];
    public int TtlMinutes { get; init; } = 30;
}

public sealed record ReserveLineItem
{
    public Guid VariantId { get; init; }
    public int Quantity { get; init; }
}
```

Wait — namespace should NOT include `Contracts`. Fix:

```csharp
namespace Module.Inventory.Features.Storefront.ReserveCartStock;

public sealed record ReserveCartStockCommand : ICommand<ReserveCartStockResponse>
{
    public Guid CartId { get; init; }
    public IReadOnlyList<ReserveLineItem> LineItems { get; init; } = [];
    public int TtlMinutes { get; init; } = 30;
}

public sealed record ReserveLineItem
{
    public Guid VariantId { get; init; }
    public int Quantity { get; init; }
}
```

- [ ] **Step 2: Create response file**

Create `Module/Inventory/Features/Storefront/ReserveCartStock/ReserveCartStock.Response.cs`:

```csharp
namespace Module.Inventory.Features.Storefront.ReserveCartStock;

public sealed record ReserveCartStockResponse
{
    public IReadOnlyList<Guid> ReservationIds { get; init; } = [];
}
```

- [ ] **Step 3: Move handler file**

Move `Module/Inventory/Features/Storefront/Contracts/ReserveCartStock/ReserveCartStock.cs` to `Module/Inventory/Features/Storefront/ReserveCartStock/ReserveCartStock.cs`.

Update namespace from `Module.Inventory.Features.Storefront.Contracts.ReserveCartStock` to `Module.Inventory.Features.Storefront.ReserveCartStock`.

Remove `using Shared.Application.Contracts.Inventory;` — the types are now in the same namespace.

- [ ] **Step 4: Delete old Shared contract**

Delete `Shared/Application/Contracts/Inventory/ReserveCartStockCommand.cs`.

- [ ] **Step 5: Build verification**

Run: `dotnet build service/Api/src/Api`
Expected: PASS (consumers still reference old namespace — will fail, fix in Task 10)

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "refactor(inventory): move ReserveCartStock from Shared to feature folder"
```

---

## Task 2: Inventory — ConsumeCartStockReservations

**Files:**
- Create: `Module/Inventory/Features/Storefront/ConsumeCartStockReservations/ConsumeCartStockReservations.Command.cs`
- Move: `Module/Inventory/Features/Storefront/Contracts/ConsumeCartStockReservations/ConsumeCartStockReservations.cs` → `Module/Inventory/Features/Storefront/ConsumeCartStockReservations/ConsumeCartStockReservations.cs`
- Delete: `Shared/Application/Contracts/Inventory/ConsumeCartStockReservationsCommand.cs`

- [x] **Step 1: Create command file**

Create `Module/Inventory/Features/Storefront/ConsumeCartStockReservations/ConsumeCartStockReservations.Command.cs`:

```csharp
namespace Module.Inventory.Features.Storefront.ConsumeCartStockReservations;

public sealed record ConsumeCartStockReservationsCommand : ICommand
{
    public Guid CartId { get; init; }
}
```

- [x] **Step 2: Move handler file**

Move `Module/Inventory/Features/Storefront/Contracts/ConsumeCartStockReservations/ConsumeCartStockReservations.cs` to `Module/Inventory/Features/Storefront/ConsumeCartStockReservations/ConsumeCartStockReservations.cs`.

Update namespace to `Module.Inventory.Features.Storefront.ConsumeCartStockReservations`.

Remove `using Shared.Application.Contracts.Inventory;`.

- [x] **Step 3: Delete old Shared contract**

Delete `Shared/Application/Contracts/Inventory/ConsumeCartStockReservationsCommand.cs`.

- [x] **Step 4: Commit**

```bash
git add -A
git commit -m "refactor(inventory): move ConsumeCartStockReservations from Shared to feature folder"
```

---

## Task 3: Inventory — ReleaseCartStockReservations

**Files:**
- Create: `Module/Inventory/Features/Storefront/ReleaseCartStockReservations/ReleaseCartStockReservations.Command.cs`
- Move: `Module/Inventory/Features/Storefront/Contracts/ReleaseCartStockReservations/ReleaseCartStockReservations.cs` → `Module/Inventory/Features/Storefront/ReleaseCartStockReservations/ReleaseCartStockReservations.cs`
- Delete: `Shared/Application/Contracts/Inventory/ReleaseCartStockReservationsCommand.cs`

- [ ] **Step 1: Create command file**

Create `Module/Inventory/Features/Storefront/ReleaseCartStockReservations/ReleaseCartStockReservations.Command.cs`:

```csharp
namespace Module.Inventory.Features.Storefront.ReleaseCartStockReservations;

public sealed record ReleaseCartStockReservationsCommand : ICommand
{
    public Guid CartId { get; init; }
}
```

- [ ] **Step 2: Move handler file**

Move `Module/Inventory/Features/Storefront/Contracts/ReleaseCartStockReservations/ReleaseCartStockReservations.cs` to `Module/Inventory/Features/Storefront/ReleaseCartStockReservations/ReleaseCartStockReservations.cs`.

Update namespace to `Module.Inventory.Features.Storefront.ReleaseCartStockReservations`.

Remove `using Shared.Application.Contracts.Inventory;`.

- [ ] **Step 3: Delete old Shared contract**

Delete `Shared/Application/Contracts/Inventory/ReleaseCartStockReservationsCommand.cs`.

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "refactor(inventory): move ReleaseCartStockReservations from Shared to feature folder"
```

---

## Task 4: Inventory — CheckVariantAvailability

**Files:**
- Create: `Module/Inventory/Features/Storefront/CheckVariantAvailability/CheckVariantAvailability.Query.cs`
- Create: `Module/Inventory/Features/Storefront/CheckVariantAvailability/CheckVariantAvailability.Response.cs`
- Move: `Module/Inventory/Features/Storefront/Contracts/CheckVariantAvailability/CheckVariantAvailability.cs` → `Module/Inventory/Features/Storefront/CheckVariantAvailability/CheckVariantAvailability.cs`
- Delete: `Shared/Application/Contracts/Inventory/CheckVariantAvailabilityQuery.cs`

- [ ] **Step 1: Create query file**

Create `Module/Inventory/Features/Storefront/CheckVariantAvailability/CheckVariantAvailability.Query.cs`:

```csharp
namespace Module.Inventory.Features.Storefront.CheckVariantAvailability;

public sealed record CheckVariantAvailabilityQuery(Guid VariantId, int Quantity)
    : IQuery<CheckVariantAvailabilityResponse>;
```

- [ ] **Step 2: Create response file**

Create `Module/Inventory/Features/Storefront/CheckVariantAvailability/CheckVariantAvailability.Response.cs`:

```csharp
namespace Module.Inventory.Features.Storefront.CheckVariantAvailability;

public sealed record CheckVariantAvailabilityResponse
{
    public Guid VariantId { get; init; }
    public bool IsAvailable { get; init; }
}
```

- [ ] **Step 3: Move handler file**

Move `Module/Inventory/Features/Storefront/Contracts/CheckVariantAvailability/CheckVariantAvailability.cs` to `Module/Inventory/Features/Storefront/CheckVariantAvailability/CheckVariantAvailability.cs`.

Update namespace to `Module.Inventory.Features.Storefront.CheckVariantAvailability`.

Remove `using Shared.Application.Contracts.Inventory;`.

- [ ] **Step 4: Delete old Shared contract**

Delete `Shared/Application/Contracts/Inventory/CheckVariantAvailabilityQuery.cs`.

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "refactor(inventory): move CheckVariantAvailability from Shared to feature folder"
```

---

## Task 5: Inventory — IStockQuantityService + Delete Contracts

**Files:**
- Move: `Shared/Application/Contracts/Inventory/IStockQuantityService.cs` → `Module/Inventory/Services/Abstractions/IStockQuantityService.cs`
- Delete: `Module/Inventory/Features/Storefront/Contracts/` (entire directory)

- [ ] **Step 1: Move interface**

Move `Shared/Application/Contracts/Inventory/IStockQuantityService.cs` to `Module/Inventory/Services/Abstractions/IStockQuantityService.cs`.

Update namespace from `Shared.Application.Contracts.Inventory` to `Module.Inventory.Services.Abstractions`.

- [ ] **Step 2: Delete Contracts/ directory**

```bash
rm -rf service/Api/src/Module/Inventory/Features/Storefront/Contracts/
```

- [ ] **Step 3: Build verification**

Run: `dotnet build service/Api/src/Module`
Expected: PASS (consumers still reference old namespace — will fail at Api level, fix in Task 10)

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "refactor(inventory): move IStockQuantityService to Services/Abstractions, delete Contracts/"
```

---

## Task 6: Ordering — GetCartForCheckout

**Files:**
- Create: `Module/Ordering/Features/Storefront/GetCartForCheckout/GetCartForCheckout.Query.cs`
- Create: `Module/Ordering/Features/Storefront/GetCartForCheckout/GetCartForCheckout.Response.cs`
- Move: `Module/Ordering/Features/Storefront/Contracts/GetCartForCheckout/GetCartForCheckout.cs` → `Module/Ordering/Features/Storefront/GetCartForCheckout/GetCartForCheckout.cs`
- Delete: `Shared/Application/Contracts/Ordering/GetCartForCheckoutQuery.cs`

- [ ] **Step 1: Create query file**

Create `Module/Ordering/Features/Storefront/GetCartForCheckout/GetCartForCheckout.Query.cs`:

```csharp
namespace Module.Ordering.Features.Storefront.GetCartForCheckout;

public sealed record GetCartForCheckoutQuery : IQuery<GetCartForCheckoutResponse>
{
    public Guid CartId { get; init; }
}
```

- [ ] **Step 2: Create response file**

Create `Module/Ordering/Features/Storefront/GetCartForCheckout/GetCartForCheckout.Response.cs`:

```csharp
namespace Module.Ordering.Features.Storefront.GetCartForCheckout;

public sealed record GetCartForCheckoutResponse
{
    public string State { get; init; } = default!;
    public IReadOnlyList<CartLineItem> LineItems { get; init; } = [];
    public decimal Total { get; init; }
    public string? Email { get; init; }
}

public sealed record CartLineItem
{
    public Guid VariantId { get; init; }
    public int Quantity { get; init; }
}
```

- [ ] **Step 3: Move handler file**

Move `Module/Ordering/Features/Storefront/Contracts/GetCartForCheckout/GetCartForCheckout.cs` to `Module/Ordering/Features/Storefront/GetCartForCheckout/GetCartForCheckout.cs`.

Update namespace to `Module.Ordering.Features.Storefront.GetCartForCheckout`.

Remove `using Shared.Application.Contracts.Ordering;`.

- [ ] **Step 4: Delete old Shared contract**

Delete `Shared/Application/Contracts/Ordering/GetCartForCheckoutQuery.cs`.

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "refactor(ordering): move GetCartForCheckout from Shared to feature folder"
```

---

## Task 7: Ordering — AdvanceCheckoutState

**Files:**
- Create: `Module/Ordering/Features/Storefront/AdvanceCheckoutState/AdvanceCheckoutState.Command.cs`
- Move: `Module/Ordering/Features/Storefront/Contracts/AdvanceCheckoutState/AdvanceCheckoutState.cs` → `Module/Ordering/Features/Storefront/AdvanceCheckoutState/AdvanceCheckoutState.cs`
- Delete: `Shared/Application/Contracts/Ordering/AdvanceCheckoutStateCommand.cs`

- [ ] **Step 1: Create command file**

Create `Module/Ordering/Features/Storefront/AdvanceCheckoutState/AdvanceCheckoutState.Command.cs`:

```csharp
namespace Module.Ordering.Features.Storefront.AdvanceCheckoutState;

public sealed record AdvanceCheckoutStateCommand : ICommand
{
    public Guid CartId { get; init; }
    public string TargetState { get; init; } = default!;
}
```

- [ ] **Step 2: Move handler file**

Move `Module/Ordering/Features/Storefront/Contracts/AdvanceCheckoutState/AdvanceCheckoutState.cs` to `Module/Ordering/Features/Storefront/AdvanceCheckoutState/AdvanceCheckoutState.cs`.

Update namespace to `Module.Ordering.Features.Storefront.AdvanceCheckoutState`.

Remove `using Shared.Application.Contracts.Ordering;`.

- [ ] **Step 3: Delete old Shared contract**

Delete `Shared/Application/Contracts/Ordering/AdvanceCheckoutStateCommand.cs`.

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "refactor(ordering): move AdvanceCheckoutState from Shared to feature folder"
```

---

## Task 8: Ordering — GetCartForShipping + Delete Contracts

**Files:**
- Create: `Module/Ordering/Features/Storefront/GetCartForShipping/GetCartForShipping.Query.cs`
- Create: `Module/Ordering/Features/Storefront/GetCartForShipping/GetCartForShipping.Response.cs`
- Move: `Module/Ordering/Features/Storefront/Contracts/GetCartForShipping/GetCartForShipping.cs` → `Module/Ordering/Features/Storefront/GetCartForShipping/GetCartForShipping.cs`
- Delete: `Shared/Application/Contracts/Ordering/GetCartForShippingQuery.cs`
- Delete: `Module/Ordering/Features/Storefront/Contracts/` (entire directory)

- [ ] **Step 1: Create query file**

Create `Module/Ordering/Features/Storefront/GetCartForShipping/GetCartForShipping.Query.cs`:

```csharp
namespace Module.Ordering.Features.Storefront.GetCartForShipping;

public sealed record GetCartForShippingQuery(Guid CartId) : IQuery<CartForShippingResponse>;
```

- [ ] **Step 2: Create response file**

Create `Module/Ordering/Features/Storefront/GetCartForShipping/GetCartForShipping.Response.cs`:

```csharp
namespace Module.Ordering.Features.Storefront.GetCartForShipping;

public sealed record CartForShippingResponse
{
    public decimal TotalWeight { get; init; }
    public decimal TotalValue { get; init; }
    public Guid? ShipAddressId { get; init; }
    public string Currency { get; init; } = default!;
}
```

- [ ] **Step 3: Move handler file and rewrite weight calculation**

Move `Module/Ordering/Features/Storefront/Contracts/GetCartForShipping/GetCartForShipping.cs` to `Module/Ordering/Features/Storefront/GetCartForShipping/GetCartForShipping.cs`.

Update namespace to `Module.Ordering.Features.Storefront.GetCartForShipping`.

Remove `using Shared.Application.Contracts.Ordering;` and `using Shared.Application.Contracts.Catalog;`.

Replace the MediatR weights query (lines 25-28) with direct DB query:

```csharp
var variantWeights = await dbContext.Set<Catalog.Domain.Products.Variants.Variant>()
    .Where(v => variantIds.Contains(v.Id))
    .Select(v => new { v.Id, v.Weight })
    .ToListAsync(cancellationToken);
var weightMap = variantWeights.ToDictionary(v => v.Id, v => v.Weight ?? 0m);
```

- [ ] **Step 4: Delete old Shared contract and Contracts/ directory**

Delete `Shared/Application/Contracts/Ordering/GetCartForShippingQuery.cs`.

```bash
rm -rf service/Api/src/Module/Ordering/Features/Storefront/Contracts/
```

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "refactor(ordering): move GetCartForShipping, rewrite weight calc, delete Contracts/"
```

---

## Task 9: Payment — GetPaymentForCheckout

**Files:**
- Create: `Module/Payment/Features/Storefront/GetPaymentForCheckout/GetPaymentForCheckout.Query.cs`
- Create: `Module/Payment/Features/Storefront/GetPaymentForCheckout/GetPaymentForCheckout.Response.cs`
- Move: `Module/Payment/Features/Storefront/Contracts/GetPaymentForCheckout/GetPaymentForCheckout.cs` → `Module/Payment/Features/Storefront/GetPaymentForCheckout/GetPaymentForCheckout.cs`
- Delete: `Shared/Application/Contracts/Payment/GetPaymentForCheckoutQuery.cs`

- [ ] **Step 1: Create query file**

Create `Module/Payment/Features/Storefront/GetPaymentForCheckout/GetPaymentForCheckout.Query.cs`:

```csharp
namespace Module.Payment.Features.Storefront.GetPaymentForCheckout;

public sealed record GetPaymentForCheckoutQuery : IQuery<PaymentForCheckoutResponse>
{
    public string PaymentIntentId { get; init; } = default!;
    public Guid OrderId { get; init; }
}
```

- [ ] **Step 2: Create response file**

Create `Module/Payment/Features/Storefront/GetPaymentForCheckout/GetPaymentForCheckout.Response.cs`:

```csharp
namespace Module.Payment.Features.Storefront.GetPaymentForCheckout;

public sealed record PaymentForCheckoutResponse
{
    public decimal Amount { get; init; }
    public bool IsCompleted { get; init; }
}
```

- [ ] **Step 3: Move handler file**

Move `Module/Payment/Features/Storefront/Contracts/GetPaymentForCheckout/GetPaymentForCheckout.cs` to `Module/Payment/Features/Storefront/GetPaymentForCheckout/GetPaymentForCheckout.cs`.

Update namespace to `Module.Payment.Features.Storefront.GetPaymentForCheckout`.

Remove `using Shared.Application.Contracts.Payment;`.

- [ ] **Step 4: Delete old Shared contract**

Delete `Shared/Application/Contracts/Payment/GetPaymentForCheckoutQuery.cs`.

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "refactor(payment): move GetPaymentForCheckout from Shared to feature folder"
```

---

## Task 10: Payment — MarkPaymentPaid + Delete Contracts

**Files:**
- Create: `Module/Payment/Features/Storefront/MarkPaymentPaid/MarkPaymentPaid.Command.cs`
- Move: `Module/Payment/Features/Storefront/Contracts/MarkPaymentPaid/MarkPaymentPaid.cs` → `Module/Payment/Features/Storefront/MarkPaymentPaid/MarkPaymentPaid.cs`
- Delete: `Shared/Application/Contracts/Payment/MarkPaymentPaidCommand.cs`
- Delete: `Module/Payment/Features/Storefront/Contracts/` (entire directory)

- [ ] **Step 1: Create command file**

Create `Module/Payment/Features/Storefront/MarkPaymentPaid/MarkPaymentPaid.Command.cs`:

```csharp
namespace Module.Payment.Features.Storefront.MarkPaymentPaid;

public sealed record MarkPaymentPaidCommand : ICommand
{
    public Guid OrderId { get; init; }
    public string PaymentIntentId { get; init; } = default!;
}
```

- [ ] **Step 2: Move handler file**

Move `Module/Payment/Features/Storefront/Contracts/MarkPaymentPaid/MarkPaymentPaid.cs` to `Module/Payment/Features/Storefront/MarkPaymentPaid/MarkPaymentPaid.cs`.

Update namespace to `Module.Payment.Features.Storefront.MarkPaymentPaid`.

Remove `using Shared.Application.Contracts.Payment;`.

- [ ] **Step 3: Delete old Shared contract and Contracts/ directory**

Delete `Shared/Application/Contracts/Payment/MarkPaymentPaidCommand.cs`.

```bash
rm -rf service/Api/src/Module/Payment/Features/Storefront/Contracts/
```

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "refactor(payment): move MarkPaymentPaid, delete Contracts/"
```

---

## Task 11: Catalog — Delete Dead Queries

**Files:**
- Delete: `Shared/Application/Contracts/Catalog/GetVariantDiscontinuedStatusesQuery.cs`
- Delete: `Shared/Application/Contracts/Catalog/GetVariantWeightsQuery.cs`
- Delete: `Shared/Application/Contracts/Catalog/` (directory)
- Update: `Module/Ordering/.../Cart/Checkout/CreateOrderFromCart.cs`

- [ ] **Step 1: Delete query contracts**

```bash
rm service/Api/src/Shared/Application/Contracts/Catalog/GetVariantDiscontinuedStatusesQuery.cs
rm service/Api/src/Shared/Application/Contracts/Catalog/GetVariantWeightsQuery.cs
rmdir service/Api/src/Shared/Application/Contracts/Catalog/
```

- [ ] **Step 2: Remove discontinued check from CreateOrderFromCart**

In `Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs`:

Remove `using Shared.Application.Contracts.Catalog;`

Remove the entire block (lines ~66-78):
```csharp
// Validate: Reject orders containing discontinued variants via ISender.
var variantIds = cart.LineItems.Select(li => li.VariantId).ToList();
var discResult = await sender.Send(new GetVariantDiscontinuedStatusesQuery { VariantIds = variantIds }, cancellationToken);
if (discResult.IsFailure)
    return discResult.Errors;

var discontinuedVariantIds = discResult.Value!
    .Where(kvp => kvp.Value)
    .Select(kvp => kvp.Key)
    .ToHashSet();

if (!cart.EnsureLineItemVariantsAreNotDiscontinued(discontinuedVariantIds))
    return OrderResult.Errors.VariantDiscontinued;
```

- [ ] **Step 3: Commit**

```bash
git add -A
git commit -m "refactor(catalog): delete dead GetVariantDiscontinuedStatusesQuery and GetVariantWeightsQuery"
```

---

## Task 12: Profile — Move Command into CreateProfile

**Files:**
- Create: `Module/Profile/Features/Storefront/Profiles/Create/CreateProfile.Command.cs`
- Update: `Module/Profile/Features/Storefront/Profiles/Create/CreateProfile.cs`
- Delete: `Shared/Application/Contracts/Profile/CreateUserProfileCommand.cs`

- [ ] **Step 1: Create command file**

Create `Module/Profile/Features/Storefront/Profiles/Create/CreateProfile.Command.cs`:

```csharp
namespace Module.Profile.Features.Storefront.Profiles.Create;

public sealed record CreateUserProfileCommand : ICommand<CreateUserProfileResult>
{
    public Guid UserId { get; init; }
    public string FirstName { get; init; } = default!;
    public string? LastName { get; init; }
    public string Email { get; init; } = default!;
}

public sealed record CreateUserProfileResult
{
    public Guid ProfileId { get; init; }
}
```

- [ ] **Step 2: Update CreateProfile.cs**

In `Module/Profile/Features/Storefront/Profiles/Create/CreateProfile.cs`:

Remove `using Shared.Application.Contracts.Profile;`

- [ ] **Step 3: Delete old Shared contract**

Delete `Shared/Application/Contracts/Profile/CreateUserProfileCommand.cs`.

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "refactor(profile): move CreateUserProfileCommand into CreateProfile feature"
```

---

## Task 13: Location — Move Queries into Handler Files

**Files:**
- Update: `Module/Location/Features/Shared/Queries/CountryExistsByIsoHandler.cs`
- Update: `Module/Location/Features/Shared/Queries/StateExistsByIsoHandler.cs`
- Delete: `Shared/Application/Contracts/Location/CountryExistsByIsoQuery.cs`
- Delete: `Shared/Application/Contracts/Location/StateExistsByIsoQuery.cs`
- Delete: `Shared/Application/Contracts/Location/` (directory)

- [ ] **Step 1: Add query record to CountryExistsByIsoHandler**

In `Module/Location/Features/Shared/Queries/CountryExistsByIsoHandler.cs`:

Add before the handler class:

```csharp
public sealed record CountryExistsByIsoQuery(string IsoCode) : IQuery<bool>;
```

Remove `using Shared.Application.Contracts.Location;`.

- [ ] **Step 2: Add query record to StateExistsByIsoHandler**

In `Module/Location/Features/Shared/Queries/StateExistsByIsoHandler.cs`:

Add before the handler class:

```csharp
public sealed record StateExistsByIsoQuery(string CountryCode, string StateCode) : IQuery<bool>;
```

Remove `using Shared.Application.Contracts.Location;`.

- [ ] **Step 3: Delete old Shared contracts and directory**

```bash
rm service/Api/src/Shared/Application/Contracts/Location/CountryExistsByIsoQuery.cs
rm service/Api/src/Shared/Application/Contracts/Location/StateExistsByIsoQuery.cs
rmdir service/Api/src/Shared/Application/Contracts/Location/
```

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "refactor(location): move query records into handler files, delete Contracts/"
```

---

## Task 14: Update All Consumer Using Statements

This task updates every file that references the old `Shared.Application.Contracts.*` namespaces. Execute in order by module.

- [ ] **Step 1: Update Inventory consumers (self-cleanup)**

Update these 4 handler files — remove `using Shared.Application.Contracts.Inventory;`:
- `Module/Inventory/Features/Storefront/ConsumeCartStockReservations/ConsumeCartStockReservations.cs`
- `Module/Inventory/Features/Storefront/ReserveCartStock/ReserveCartStock.cs`
- `Module/Inventory/Features/Storefront/ReleaseCartStockReservations/ReleaseCartStockReservations.cs`
- `Module/Inventory/Features/Storefront/CheckVariantAvailability/CheckVariantAvailability.cs`

- [ ] **Step 2: Update Ordering consumers**

`Module/Ordering/.../Cart/Checkout/CreateOrderFromCart.cs`:
```csharp
// Remove:
using Shared.Application.Contracts.Catalog;
using Shared.Application.Contracts.Inventory;
using Shared.Application.Contracts.Payment;

// Add:
using Module.Inventory.Features.Storefront.ConsumeCartStockReservations;
using Module.Payment.Features.Storefront.GetPaymentForCheckout;
using Module.Payment.Features.Storefront.MarkPaymentPaid;
```

`Module/Ordering/.../Cart/AddItem/AddToCart.cs`:
```csharp
// Remove:
using Shared.Application.Contracts.Inventory;

// Add:
using Module.Inventory.Features.Storefront.ReserveCartStock;
```

`Module/Ordering/.../Cart/UpdateItemQuantity/UpdateCartItemQuantity.cs`:
```csharp
// Remove:
using Shared.Application.Contracts.Inventory;

// Add:
using Module.Inventory.Features.Storefront.CheckVariantAvailability;
```

`Module/Ordering/.../GetCartForShipping/GetCartForShipping.cs`:
```csharp
// Remove:
using Shared.Application.Contracts.Catalog;
using Shared.Application.Contracts.Ordering;

// (no new usings needed — Query/Response are in same namespace)
```

- [ ] **Step 3: Update Payment consumer**

`Module/Payment/.../Payment/CreateIntent/CreatePaymentIntent.cs`:
```csharp
// Remove:
using Shared.Application.Contracts.Ordering;
using Shared.Application.Contracts.Inventory;

// Add:
using Module.Inventory.Features.Storefront.ReserveCartStock;
using Module.Inventory.Features.Storefront.ReleaseCartStockReservations;
using Module.Ordering.Features.Storefront.GetCartForCheckout;
using Module.Ordering.Features.Storefront.AdvanceCheckoutState;
```

- [ ] **Step 4: Update Profile consumer**

`Module/Profile/.../Addresses/Validators/Address.Validator.cs`:
```csharp
// Remove:
// (uses Shared.Application.Contracts.Location indirectly via sender.Send)

// Add:
using Module.Location.Features.Shared.Queries;
```

- [ ] **Step 5: Update Identity consumers**

`Module/Identity/.../Emails/Confirm/ConfirmEmail.cs`:
```csharp
// Remove:
using Shared.Application.Contracts.Profile;

// Add:
using Module.Profile.Features.Storefront.Profiles.Create;
```

`Module/Identity/.../Auth/Login/External/Authenticate/ExternalAuthenticate.cs`:
```csharp
// Remove:
using Shared.Application.Contracts.Profile;

// Add:
using Module.Profile.Features.Storefront.Profiles.Create;
```

- [ ] **Step 6: Build verification**

Run: `dotnet build service/Api/src/Api`
Expected: PASS with 0 warnings

- [ ] **Step 7: Commit**

```bash
git add -A
git commit -m "refactor: update all consumer using statements for relocated contracts"
```

---

## Task 15: Update Test Files

- [ ] **Step 1: Update CreateOrderFromCartTransactionTests.cs**

Remove `using Shared.Application.Contracts.Catalog;` and `using Shared.Application.Contracts.Inventory;` and `using Shared.Application.Contracts.Payment;`.

Add:
```csharp
using Module.Inventory.Features.Storefront.ConsumeCartStockReservations;
using Module.Payment.Features.Storefront.GetPaymentForCheckout;
using Module.Payment.Features.Storefront.MarkPaymentPaid;
```

Remove the `GetVariantDiscontinuedStatusesQuery` mock setup (line ~57).

- [ ] **Step 2: Update CreateOrderFromCartTests.cs**

Same using changes as Step 1.

Remove `GetVariantDiscontinuedStatusesQuery` mock setups (lines ~62, ~243).

- [ ] **Step 3: Update CreateOrderFromCartStockTests.cs**

Same using changes as Step 1.

Remove `GetVariantDiscontinuedStatusesQuery` mock setup (line ~54).

- [ ] **Step 4: Update AddToCart test files**

`AddToCartTests.cs`, `AddToCartDefaultsTests.cs`, `AddToCart.Reservation.Tests.cs`:

Replace `using Shared.Application.Contracts.Inventory;` with `using Module.Inventory.Features.Storefront.ReserveCartStock;`.

- [ ] **Step 5: Update UpdateCartItemQuantity.Tests.cs**

Replace `using Shared.Application.Contracts.Inventory;` with `using Module.Inventory.Features.Storefront.CheckVariantAvailability;`.

- [ ] **Step 6: Update CreatePaymentIntentTests.cs**

Replace:
```csharp
using Shared.Application.Contracts.Ordering;
using Shared.Application.Contracts.Inventory;
```
With:
```csharp
using Module.Ordering.Features.Storefront.GetCartForCheckout;
using Module.Ordering.Features.Storefront.AdvanceCheckoutState;
using Module.Inventory.Features.Storefront.ReserveCartStock;
using Module.Inventory.Features.Storefront.ReleaseCartStockReservations;
```

- [ ] **Step 7: Update remaining test files**

`UpdateOrderStatusTests.cs`: Replace `using Shared.Application.Contracts.Inventory;` with `using Module.Inventory.Services.Abstractions;`

`EventHandlerInvocationTests.cs`: Same as above.

`CheckVariantAvailability.Tests.cs`: Replace with `using Module.Inventory.Features.Storefront.CheckVariantAvailability;`

`CalculateShippingHandlerTests.cs`: Replace `using Shared.Application.Contracts.{Catalog,Ordering};` with `using Module.Ordering.Features.Storefront.GetCartForShipping;`

`ExistsByIsoQueryTests.cs`: Replace with `using Module.Location.Features.Shared.Queries;`

`ConfirmEmail.Tests.cs`, `ExternalAuthenticate.Tests.cs`, `ExternalAuthenticateProfileCreationTests.cs`: Replace `using Shared.Application.Contracts.Profile;` with `using Module.Profile.Features.Storefront.Profiles.Create;`

- [ ] **Step 8: Build and test**

Run: `dotnet build service/Api/src/Api`
Run: `dotnet test service/Api/tests/Module.UnitTests`
Expected: All PASS

- [ ] **Step 9: Commit**

```bash
git add -A
git commit -m "refactor: update test using statements for relocated contracts"
```

---

## Task 16: Final Cleanup and Verification

- [ ] **Step 1: Delete remaining Shared Contracts directories**

```bash
rmdir service/Api/src/Shared/Application/Contracts/Inventory/ 2>/dev/null || true
rmdir service/Api/src/Shared/Application/Contracts/Ordering/ 2>/dev/null || true
rmdir service/Api/src/Shared/Application/Contracts/Payment/ 2>/dev/null || true
rmdir service/Api/src/Shared/Application/Contracts/Profile/ 2>/dev/null || true
rmdir service/Api/src/Shared/Application/Contracts/Location/ 2>/dev/null || true
rmdir service/Api/src/Shared/Application/Contracts/Catalog/ 2>/dev/null || true
```

- [ ] **Step 2: Update cross-module baseline**

In `scripts/check-cross-module-refs.sh`, update `EXPECTED_BASELINE` to reflect the new count (run the script first to see the actual count).

- [ ] **Step 3: Full build verification**

Run: `dotnet build service/Api/src/Api`
Expected: 0 warnings, 0 errors

- [ ] **Step 4: Full test verification**

Run: `dotnet test service/Api/tests/Module.UnitTests`
Run: `dotnet test service/Api/tests/Shared.UnitTests`
Expected: All PASS

- [ ] **Step 5: Cross-module check**

Run: `bash scripts/check-cross-module-refs.sh`
Expected: Baseline updated, no new violations

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "chore: cleanup empty Shared/Contracts directories, update cross-module baseline"
```
