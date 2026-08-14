---
goal: Replace the plain enum→string converters on Order status columns with bidirectional value converters (read legacy, write canonical) and consolidate the in-progress migrations into a single clean migration.
version: 1.0
date_created: 2026-08-14
last_updated: 2026-08-14
owner: Ordering
status: 'Planned'
tags: [refactor, ordering, enum, value-converter, migration]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

The `CheckoutState`/`OrderPaymentState`/`OrderShipmentState` enums were renamed and
the entity properties re-typed (done), but `OrderConfiguration` still uses plain
`.HasConversion<string>()` (stores the enum member name). Existing rows persisted
under the OLD names (`"Delivery"`, `"Payment"`, `"balance_due"`, `"pending"`, …)
will fail `Enum.Parse` on read. This plan replaces the plain converters with
bidirectional value converters that read legacy + canonical names and write
canonical, and consolidates the two in-progress migrations (`RemoveTaxCategoryIdFromShippingMethod`,
`AddPaymentBusinessTimestamps`) into one clean migration.

**Spec:** `spec/spec-checkout-state-enum-alignment.md` §3.7, §4.6

## 1. Requirements & Constraints

- **REQ-001**: `OrderConfiguration` maps `CheckoutState`, `PaymentState`, `ShipmentState` with bidirectional `ValueConverter<enum,string>`: write = canonical enum name; read = accept canonical name and every legacy name (§4.6 table).
- **REQ-002**: The three status columns remain `text` (no schema type change).
- **REQ-003**: Drop the committed migrations `20260813090249_RemoveTaxCategoryIdFromShippingMethod` and `20260814011730_AddPaymentBusinessTimestamps` (they are already committed — `git status --short -- service/Api/src/Migrations/` is empty) and regenerate one migration carrying their schema effects.
- **CON-001**: `TreatWarningsAsErrors=true` — zero-warning build.
- **CON-002**: No destructive git (`checkout -- <path>`, `restore`, `reset --hard`) without explicit human "yes" (AGENTS.md rule 6). Not required here — the snapshot is already committed and clean.
- **PAT-001**: Nullable enum converters pass `null` through (write lambda uses `v!.ToString()`, read lambda returns `null` for `null`).

## 2. Implementation Steps

### Implementation Phase 1 — Value converters

- GOAL-001: Add bidirectional converters so legacy and canonical stored values both materialize.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Add converters to `OrderConfiguration.cs` for the three status columns. | | |
| TASK-002 | Add unit tests asserting legacy + canonical round-trip for all three enums. | | |

#### TASK-001: Bidirectional converters in `OrderConfiguration.cs`

**Files:**
- Modify: `service/Api/src/Module/Ordering/Persistence/Configurations/OrderConfiguration.cs:27-33`

Replace the three `HasConversion<string>()` lines with:

```csharp
builder.Property(x => x.CheckoutState)
    .IsRequired()
    .HasConversion(
        v => v.ToString(),
        v => v switch
        {
            "Delivery" => CheckoutState.PickDeliveryMethod,
            "Payment"  => CheckoutState.PickPaymentMethod,
            _ => Enum.Parse<CheckoutState>(v)
        })
    .HasDefaultValue(CheckoutState.Address);

builder.Property(x => x.PaymentState)
    .HasConversion(
        v => v!.ToString(),
        v => v switch
        {
            "completed"   => OrderPaymentState.Completed,
            "failed"      => OrderPaymentState.Failed,
            "void"        => OrderPaymentState.Void,
            "balance_due" => OrderPaymentState.BalanceDue,
            "credit_owed" => OrderPaymentState.CreditOwed,
            "paid"        => OrderPaymentState.Paid,
            "pending"     => OrderPaymentState.Pending,
            "checkout"    => OrderPaymentState.Checkout,
            "invalid"     => OrderPaymentState.Invalid,
            _ => Enum.Parse<OrderPaymentState>(v)
        });

builder.Property(x => x.ShipmentState)
    .HasConversion(
        v => v!.ToString(),
        v => v switch
        {
            "pending"   => OrderShipmentState.Pending,
            "delivered" => OrderShipmentState.Delivered,
            "partial"   => OrderShipmentState.Partial,
            "ready"     => OrderShipmentState.Ready,
            "backorder" => OrderShipmentState.Backorder,
            "canceled"  => OrderShipmentState.Canceled,
            _ => Enum.Parse<OrderShipmentState>(v)
        });
```

Run `dotnet build service/Api/src/Api/Api.csproj` — expected: 0 warnings. Commit.

#### TASK-002: Converter round-trip tests

**Files:**
- Create: `service/Api/tests/Module.UnitTests/Ordering/Persistence/OrderStatusValueConverterTests.cs`

Test each converter via the EF model (use an in-memory `ApplicationDbContext` seeded with a raw legacy string, read it back, assert the enum member). Cover: `CheckoutState` legacy `"Delivery"`/`"Payment"` → `PickDeliveryMethod`/`PickPaymentMethod`; `OrderPaymentState` `"balance_due"` → `BalanceDue`; `OrderShipmentState` `"ready"` → `Ready`; and a canonical round-trip. Run `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~OrderStatusValueConverter"`.

### Implementation Phase 2 — Migration consolidation

- GOAL-002: One clean migration from a clean snapshot; no data-rewrite SQL.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-003 | Remove the two in-progress migration files (requires human approval to reset the snapshot). | | |
| TASK-004 | Regenerate a single migration (`RenameCheckoutStateAndAddTimestamps`) from the corrected model. | | |

#### TASK-003: Remove the two committed migrations

**Files:**
- Delete: `service/Api/src/Migrations/Migrations/20260813090249_RemoveTaxCategoryIdFromShippingMethod.cs` + `.Designer.cs` (committed)
- Delete: `service/Api/src/Migrations/Migrations/20260814011730_AddPaymentBusinessTimestamps.cs` + `.Designer.cs` (committed)
- Then regenerate (TASK-004) — the snapshot is committed and clean, so no `git checkout --` is required.

> `RemoveTaxCategoryIdFromShippingMethod` drops `shipping_methods.tax_category_id` (a separate shipping refactor). `AddPaymentBusinessTimestamps` adds `payment_captures` business-timestamp columns (`completed_at_utc`, `failed_at_utc`, `voided_at_utc`, `disputed_at_utc`, `refunded_at_utc`, `last_stripe_event_id`, `last_stripe_event_created_at_utc`). If the shipping migration belongs to another in-progress effort, keep it and only drop `AddPaymentBusinessTimestamps` — note the dependency in §4.

#### TASK-004: Regenerate migration

```bash
dotnet ef migrations add RenameCheckoutStateAndAddTimestamps \
  --project service/Api/src/Migrations/Api.Migrations.csproj \
  --startup-project service/Api/src/Api/Api.csproj
```

Expected: a migration carrying the `tax_category_id` drop (shipping) and the
`payment_captures` business-timestamp columns. The Order `Payment*`/`ShippedAt`/
`DeliveredAt`/`EstimatedDeliveryAt`/`DeliveryExceptionAt` columns already exist in
`20260812111403_InitialCreate.cs` and are NOT re-added. The value converters do not
alter the status columns' text type, so they produce no schema diff. Verify
`dotnet build` (0 warnings) and `dotnet test service/Api/tests/Module.UnitTests`.

## 3. Alternatives

- **ALT-001**: SQL `UPDATE` backfill in the migration. Rejected — non-destructive read-mapping keeps legacy rows valid indefinitely and lazy-canonicalizes on next write.
- **ALT-002**: Keep plain `.HasConversion<string>()` and accept read failures on legacy data. Rejected — breaks any existing DB.

## 4. Dependencies

- **DEP-001**: The enum rename (already applied) — `CheckoutState`/`OrderPaymentState`/`OrderShipmentState` and typed `Order` properties.
- **DEP-002**: `feature-shipment-aggregate-1` will later rename `OrderShipmentState` → `OrderFulfillmentState`; this plan's `ShipmentState` converter is transient until then.

## 5. Files

- **FILE-001**: `service/Api/src/Module/Ordering/Persistence/Configurations/OrderConfiguration.cs` (TASK-001).
- **FILE-002**: `service/Api/tests/Module.UnitTests/Ordering/Persistence/OrderStatusValueConverterTests.cs` (TASK-002).
- **FILE-003**: `service/Api/src/Migrations/Migrations/*` (TASK-003/004).

## 6. Testing

- **TEST-001**: Legacy + canonical round-trip tests (TASK-002) pass.
- **TEST-002**: `dotnet build` 0 warnings.
- **TEST-003**: `dotnet test service/Api/tests/Module.UnitTests` green.
- **TEST-004**: `bash scripts/check-feature-conventions.sh` passes.

## 7. Risks & Assumptions

- **RISK-001**: Dropping committed migrations rewrites history; confirm `RemoveTaxCategoryIdFromShippingMethod` is owned by this effort (else keep it and only drop `AddPaymentBusinessTimestamps`). No git destructive command is required.
- **ASSUMPTION-001**: The two migrations are committed and the snapshot is clean (verified).

## 8. Related Specifications / Further Reading

- [spec-checkout-state-enum-alignment.md](../spec/spec-checkout-state-enum-alignment.md) §3.7, §4.6
- [refactor-checkout-state-enum-alignment-1.md](./refactor-checkout-state-enum-alignment-1.md) — the (now-applied) backend rename.
