---
goal: Correct EF Core entity relationships across all modules and regenerate a fresh squashed InitialCreate migration
version: 1.0
date_created: 2026-08-16
owner: Platform/Data
status: 'In Progress'
tags: ['data', 'architecture', 'migration', 'efcore', 'refactor']
---

# Introduction

![Status: In Progress](https://img.shields.io/badge/status-In_Progress-yellow)

Audit found relationship defects across the 33 EF Core `IEntityTypeConfiguration` files: a missing same-module FK (`ShippingMethodZone.ShippingMethodId`), multiple cross-module FK columns left as loose `Guid` without constraints (`Order.UserId`, `Order.BillAddressId`/`ShipAddressId`, `Order.ShippingRateId`, `Shipment.OrderId`/`AddressId`, `StockItem.VariantId`, `WishedItem.VariantId`, `TransferItem.VariantId`, `StockLocation.CountryId`/`StateId`, `StockReservation.*`), and convention-discovered relationships left implicit with accidental cascade delete behavior. The architecture rule prohibiting cross-module references is being dropped, so cross-module FKs are now intentional and must be declared explicitly. Per the owner's directive (2026-08-16), all relationships are modeled with **real navigation/relationship properties on the domain entities** and configured as `HasOne(x => x.Nav).WithMany(...)`, replacing anonymous `HasOne<TTarget>()` calls. The existing squashed `InitialCreate` migration will be removed and regenerated from the corrected model.

## 1. Requirements & Constraints

- **REQ-001**: Every cross-module FK column listed in section 2 must map to an explicit relationship configured through a domain navigation property, with `DeleteBehavior.Restrict` (no accidental cascade).
- **REQ-002**: Keep the cross-module relationships that already exist (`LineItem → Variant`, `Order → PaymentMethod`, `Order → ShippingMethod`, `PaymentCapture → Order`); do not remove them.
- **REQ-003**: Add the missing same-module FK `ShippingMethodZone.ShippingMethodId → shipping.shipping_methods`.
- **REQ-004**: Keep `PaymentCapture.OrderId` on `DeleteBehavior.Cascade`; keep `Product.MasterVariantId` as a loose `Guid` column with no FK and no index.
- **REQ-005**: Regenerate the migration by deleting the current `InitialCreate` files and creating a fresh one; no incremental migration.
- **REQ-006**: Update rule #2 in `AGENTS.md` and related enforcement so cross-module FKs no longer fail verification.
- **REQ-007**: All relationships use navigation properties (`HasOne(x => x.Nav).WithMany(...)`); no anonymous `HasOne<TTarget>()` calls remain anywhere in the solution.
- **SEC-001**: No credentials, connection strings, or secrets may be committed; the design-time factory connection string remains in `DesignTimeDbContextFactory.cs` only.
- **CON-001**: `TreatWarningsAsErrors=true` globally; any new warning fails `dotnet build`.
- **CON-002**: Modules still share one `Module.csproj` assembly; cross-module *namespace* references are allowed, but module code must continue communicating via MediatR `ISender` for behavior, not direct service calls.
- **CON-003**: The migration project is `Api.Migrations`; startup project for `dotnet ef` is `Api` (contains `DesignTimeDbContextFactory`).
- **GUD-001**: All relationships use navigation properties on the domain entities, configured as `builder.HasOne(x => x.Nav).WithMany(inverse => inverse.Collection).HasForeignKey(x => x.FkColumn).OnDelete(...)`.
- **GUD-002**: Keep one relationship declaration per FK; do not declare both ends of the same relationship in two config files.
- **PAT-001**: Follow the codebase `#region Relationships` block placement inside each `Configure` method.
- **PAT-002**: Use `DeleteBehavior.Restrict` for all newly added FKs unless a task explicitly states otherwise.

## 2. Implementation Steps

### Implementation Phase 1

- GOAL-001: Correct and connect EF Core relationships across the Ordering, Shipping, Inventory, Customer, and Location configuration files.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | `OrderConfiguration.cs` — add nav-property FKs in `#region Relationships`: `HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(Restrict)`, `HasOne(x => x.BillAddress).WithMany().HasForeignKey(x => x.BillAddressId).OnDelete(Restrict)`, `HasOne(x => x.ShipAddress).WithMany().HasForeignKey(x => x.ShipAddressId).OnDelete(Restrict)`, `HasOne(x => x.ShippingRate).WithMany().HasForeignKey(x => x.ShippingRateId).OnDelete(Restrict)`. Add explicit `HasOne(x => x.PaymentMethod).WithMany().HasForeignKey(x => x.PaymentMethodId).OnDelete(SetNull)` and `HasOne(x => x.ShippingMethod).WithMany().HasForeignKey(x => x.ShippingMethodId).OnDelete(Restrict)`. Add nav properties `User`, `BillAddress`, `ShipAddress`, `ShippingRate` to `Order.cs`. Add usings `Module.Customer.Domain.Addresses`, `Module.Shipping.Domain.ShippingRates`, `Shared.Security.Identity.Domain.Users`. Add indexes on `BillAddressId`, `ShipAddressId`, `ShippingRateId`. | Yes | 2026-08-16 |
| TASK-002 | `Shipment.Configuration.cs` — add nav-property FKs: `HasOne(x => x.Order).WithMany(o => o.Shipments).HasForeignKey(x => x.OrderId).OnDelete(Restrict)`, `HasOne(x => x.Address).WithMany().HasForeignKey(x => x.AddressId).OnDelete(Restrict)`, and explicit `HasOne(x => x.ShippingMethod).WithMany().HasForeignKey(x => x.ShippingMethodId).OnDelete(Restrict)`. Add nav properties `Order`, `Address` to `Shipment.cs`; add `ICollection<Shipment> Shipments` to `Order.cs`. Add usings `Module.Ordering.Domain.Orders`, `Module.Customer.Domain.Addresses`. Keep existing `ix_shipments_order_id` index. | Yes | 2026-08-16 |
| TASK-003 | `ShippingMethodZone.Configuration.cs` — add nav-property FK `HasOne(x => x.ShippingMethod).WithMany(sm => sm.ShippingMethodZones).HasForeignKey(x => x.ShippingMethodId).OnDelete(Cascade)`. Add `ShippingMethod` nav to `ShippingMethodZone.cs` and `ICollection<ShippingMethodZone> ShippingMethodZones` to `ShippingMethod.cs`. | Yes | 2026-08-16 |
| TASK-004 | `ShippingRate.Configuration.cs` — make the convention-discovered relationship explicit: `HasOne(x => x.ShippingMethod).WithMany(sm => sm.ShippingRates).HasForeignKey(x => x.ShippingMethodId).OnDelete(Restrict)`. | Yes | 2026-08-16 |
| TASK-005 | `StockItemConfiguration.cs` — add nav-property FK `HasOne(x => x.Variant).WithMany(v => v.StockItems).HasForeignKey(x => x.VariantId).OnDelete(Restrict)`. `StockItem.Variant` nav already exists; add `ICollection<StockItem> StockItems` to `Variant.cs`. Add using `Module.Catalog.Domain.Variants`. | Yes | 2026-08-16 |
| TASK-006 | `StockReservationConfiguration.cs` — add nav-property FKs: `HasOne(x => x.Variant).WithMany(v => v.StockReservations).HasForeignKey(x => x.VariantId).OnDelete(Restrict)`, `HasOne(x => x.StockLocation).WithMany(sl => sl.StockReservations).HasForeignKey(x => x.StockLocationId).OnDelete(Restrict)`, `HasOne(x => x.Order).WithMany(o => o.StockReservations).HasForeignKey(x => x.OrderId).OnDelete(Restrict)`. Add navs `Variant`, `StockLocation`, `Order` to `StockReservation.cs`; inverse collections on `Variant.cs`, `StockLocation.cs`, `Order.cs`. Add usings `Module.Catalog.Domain.Variants`, `Module.Ordering.Domain.Orders`. | Yes | 2026-08-16 |
| TASK-007 | `TransferItemConfiguration.cs` — add nav-property FK `HasOne(x => x.Variant).WithMany(v => v.TransferItems).HasForeignKey(x => x.VariantId).OnDelete(Restrict)`. Add `Variant` nav to `TransferItem` (declared in `StockTransfer.cs`); inverse collection on `Variant.cs`. Add using `Module.Catalog.Domain.Variants`. | Yes | 2026-08-16 |
| TASK-008 | `StockLocationConfiguration.cs` — add nav-property FKs: `HasOne(x => x.Country).WithMany(x => x.StockLocations).HasForeignKey(x => x.CountryId).OnDelete(Restrict)`, `HasOne(x => x.State).WithMany(x => x.StockLocations).HasForeignKey(x => x.StateId).OnDelete(Restrict)`. `StockLocation.Country`/`State` navs already exist. Add usings `Module.Location.Domain.Countries`, `Module.Location.Domain.States`. | Yes | 2026-08-16 |
| TASK-009 | `WishedItem.Configuration.cs` — add nav-property FK `HasOne(x => x.Variant).WithMany(v => v.WishedItems).HasForeignKey(x => x.VariantId).OnDelete(Restrict)`. Add `Variant` nav to `WishedItem.cs`; inverse collection on `Variant.cs`. Add using `Module.Catalog.Domain.Variants`. | Yes | 2026-08-16 |
| TASK-010 | `Wishlist.Configuration.cs` — make the convention-discovered relationship explicit: `HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(Restrict)`. Add using `Shared.Security.Identity.Domain.Users`. | Yes | 2026-08-16 |
| TASK-011 | `UserProfile.Configuration.cs` — make the convention-discovered relationships explicit: `HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(Restrict)` plus nav-property FKs for `DefaultBillingAddress` and `DefaultShippingAddress` (`SetNull`). Add navs `DefaultBillingAddress`, `DefaultShippingAddress` to `UserProfile.cs`. | Yes | 2026-08-16 |
| TASK-012 | Leave `PaymentRecordConfiguration.cs:40` (`PaymentCapture → Order`, Cascade) unchanged except converting to `HasOne(x => x.Order).WithMany(o => o.PaymentCaptures)`; add `Order` nav to `PaymentCapture.cs` and `ICollection<PaymentCapture> PaymentCaptures` to `Order.cs`. `Product.MasterVariantId` stays a loose column with no FK. | Yes | 2026-08-16 |
| TASK-026 | Fill in empty `WithMany()`/`WithOne()` calls with inverse navigation properties (owner directive 2026-08-16), and verify FK Id properties match nav names. Added inverse navs: `TransferItem.StockTransfer`, `StockLocation.SourceStockTransfers`/`DestinationStockTransfers`, `OptionValue.OptionValueVariants`, `ShippingMethod.Shipments`/`Orders`, `PaymentMethod.Orders`, `ShippingRate.Orders`, `Address.BillingOrders`/`ShippingOrders`/`Shipments`. Configs updated: `StockTransfer.Configuration.cs`, `OptionValueVariantConfiguration.cs`, `Shipment.Configuration.cs`, `OrderConfiguration.cs`. Intentional empty `WithMany()` remain only for Shared `User` (Order/Wishlist/UserProfile, forward-only dependency) and `UserProfile` DefaultBillingAddress/DefaultShippingAddress (dual pointer FKs). | Yes | 2026-08-16 |
| TASK-027 | Add the 4 new cross-referencing domain files to `scripts/check-cross-module-refs.sh` whitelist (`PaymentMethod.cs`, `ShippingMethod.cs`, `ShippingRate.cs`, `Address.cs`); baseline stays 46. `OptionValue.cs` is same-module only — no whitelist entry needed. | Yes | 2026-08-16 |
| TASK-028 | Audit all EF configs for inline (hardcoded literal) or missing column constraints; move every literal to domain `XxxConstant.Constraints`, add matching `Result` error factories and FluentValidation `ApplyXxxRules` methods. Completed across Billing (PaymentMethod Presentation=500, WebhookEvent new Constant/Result/Validation wired, PaymentRecord Stripe ids 200/checkout 2048/source 200/lastEvent 100), Shipping (Shipment TrackingNumber 255→200 to match config, ShippingMethodZone new files CountryCode regex `^([A-Z]{2}|\*)$`/StateCode 10, ShippingMethod Presentation=500), Inventory (StockMovement Action=50/OriginatorType=200, StockTransfer State=20+IsInEnum, StockReservation Reason=255), Ordering (Adjustment State=50 + open/closed IsInEnum-style Must rule), Customer (UserProfile TotalSpent precision 18,2), Catalog (Variant unit strings=10). | Yes | 2026-08-16 |
| TASK-029 | Fix `scripts/check-cross-module-refs.sh` whitelist entry `Ordering/Persistence/Configurations/OrderConfiguration.cs` → `Order.Configuration.cs` (file renamed in TASK-026; stale name dropped its 2 refs from whitelist, breaking baseline). Baseline restored to 46. | Yes | 2026-08-16 |

### Implementation Phase 2

- GOAL-002: Drop the "modules must not cross-reference" rule and update all enforcement to permit intentional cross-module FKs.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-013 | `AGENTS.md` — rewrite rule #2: modules may declare cross-module EF Core FK relationships (explicit, Restrict unless stated); behavior must still flow through MediatR `ISender`; remove the "39 known violations" statement and the `ValidateVerticalSliceIsolation` note. | Yes | 2026-08-16 |
| TASK-014 | `scripts/check-cross-module-refs.sh` — replace the fixed `EXPECTED_BASELINE=35` failure model with a whitelist of allowed cross-module references (FK configs + domain navigation files), exiting 0 when only whitelisted refs remain. Baseline recomputed to 46 after nav properties added. | Yes | 2026-08-16 |
| TASK-015 | `Directory.Build.targets` — update the `ValidateVerticalSliceIsolation` target comment and condition to note that cross-module FK *relationships* are permitted and no longer warned; keep the target functional for future true module-boundary checks. | Yes | 2026-08-16 |
| TASK-016 | `docs/codebase/ARCHITECTURE.md` and `docs/codebase/CONCERNS.md` — update the module-isolation section to document the intentional cross-module FK list (LineItem→Variant, Order→PaymentMethod/ShippingMethod/User/Addresses/ShippingRate, PaymentCapture→Order, Shipment→Order/Address, StockItem→Variant, StockReservation→Variant/StockLocation/Order, TransferItem→Variant, WishedItem→Variant, StockLocation→Country/State, ShippingMethodZone→ShippingMethod). | Yes | 2026-08-16 |

### Implementation Phase 3

- GOAL-003: Remove the existing squashed migration and generate a fresh `InitialCreate` from the corrected model.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-017 | Ensure `dotnet-ef` tool is installed: `dotnet tool install --global dotnet-ef` if `dotnet ef --version` fails. | Yes | 2026-08-16 |
| TASK-018 | Delete `service/Api/src/Migrations/Migrations/20260816090623_IntialCreate.cs` and `20260816090623_IntialCreate.Designer.cs`. The previously tracked `20260815204042_InitialCreate.*` are already deleted in the working tree. | Yes | 2026-08-16 |
| TASK-019 | Run `dotnet ef migrations remove -p src/Migrations/Api.Migrations.csproj -s src/Api/Api.csproj` from `service/Api` to revert `ApplicationDbContextModelSnapshot.cs` to a clean baseline; if no migration remains, manually truncate the snapshot's model content instead. | Yes | 2026-08-16 |
| TASK-020 | Run `dotnet ef migrations add InitialCreate -p src/Migrations/Api.Migrations.csproj -s src/Api/Api.csproj` from `service/Api` to regenerate the snapshot and migration reflecting the Phase 1 model. | Yes | 2026-08-16 |
| TASK-021 | Inspect the generated `2026XXXXXXXXXX_InitialCreate.cs` and verify: all Phase 1 FKs present with correct `OnDelete`; `fk_payment_captures_order_order_id` remains `CASCADE`; no `master_variant_id` FK/index; no duplicate FK declarations. | Yes | 2026-08-16 |

### Implementation Phase 4

- GOAL-004: Verify the corrected model builds cleanly, tests pass, and no pending model changes remain.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-022 | Run `dotnet build` from `service/Api` — must pass with zero warnings (warnings-as-errors). | Yes | 2026-08-16 |
| TASK-023 | Run `dotnet test service/Api/tests/Module.UnitTests` and `dotnet test service/Api/tests/Shared.UnitTests` — all pass. | No (pre-existing infra issue: xunit v3 MTP discovers 0 tests in both projects; unrelated to this plan) | 2026-08-16 |
| TASK-024 | Run `dotnet ef migrations has-pending-model-changes -p src/Migrations/Api.Migrations.csproj -s src/Api/Api.csproj` from `service/Api` — output must be "No changes" (model and migration in sync). | Yes | 2026-08-16 |
| TASK-025 | Run `bash scripts/check-cross-module-refs.sh` and `bash scripts/check-feature-conventions.sh` — both exit 0. | Yes | 2026-08-16 |

## 3. Alternatives

- **ALT-001**: Keep the current incremental-migration approach by adding a new `CorrectRelationships` migration on top of `InitialCreate`. Rejected: the working tree already has an untracked squashed `InitialCreate` plus a dirty snapshot; a fresh baseline is cleaner and matches the repository's squashing convention.
- **ALT-002**: Enforce cross-module isolation (remove LineItem→Variant, Order→PaymentMethod, etc.) per original rule #2. Rejected by decision: the rule is being dropped; the storefront needs these FKs for product/order/cart lookups.
- **ALT-003**: Model every cross-module FK as a shadow property instead of an anonymous FK on the existing `Guid` column. Rejected: it would rename DB columns and require data migration; nav-property `HasOne(x => x.Nav)` preserves existing column names.
- **ALT-004**: Add navigation properties on both ends of each new cross-module relationship. **Selected per owner directive**: each FK-holder entity gets a `Nav` property and the principal gets an inverse `ICollection` (or empty `WithMany()` where the principal is `Shared.Security.User` to avoid Shared→Module coupling); configs use `HasOne(x => x.Nav).WithMany(...)`.

## 4. Dependencies

- **DEP-001**: `dotnet-ef` CLI (global tool) — required by TASK-017..020, TASK-024.
- **DEP-002**: `service/Api/src/Api/DesignTimeDbContextFactory.cs` — provides the design-time `ApplicationDbContext` used by all `dotnet ef` commands; must keep compiling.
- **DEP-003**: `Shared.Security.Identity.Domain.Users.User`, `Module.Customer.Domain.Addresses.Address`, `Module.Shipping.Domain.ShippingRates.ShippingRate`, `Module.Ordering.Domain.Orders.Order`, `Module.Catalog.Domain.Variants.Variant`, `Module.Location.Domain.Countries.Country`, `Module.Location.Domain.States.State`, `Module.Inventory.Domain.StockLocations.StockLocation` — target types referenced by the new anonymous FKs; all already exist.
- **DEP-004**: Existing cross-module FK configs to replicate the pattern from: `PaymentRecordConfiguration.cs:40`, `LineItemConfiguration.cs:50-53`.

## 5. Files

- **FILE-001**: `service/Api/src/Module/Ordering/Persistence/Configurations/OrderConfiguration.cs` — add FKs for UserId, BillAddressId, ShipAddressId, ShippingRateId; make PaymentMethod/ShippingMethod FKs explicit; add indexes.
- **FILE-002**: `service/Api/src/Module/Shipping/Persistence/Configurations/Shipment.Configuration.cs` — add FKs for OrderId, AddressId; make ShippingMethod FK explicit.
- **FILE-003**: `service/Api/src/Module/Shipping/Persistence/Configurations/ShippingMethodZone.Configuration.cs` — add missing ShippingMethod FK.
- **FILE-004**: `service/Api/src/Module/Shipping/Persistence/Configurations/ShippingRate.Configuration.cs` — make ShippingMethod FK explicit.
- **FILE-005**: `service/Api/src/Module/Inventory/Persistence/Configurations/StockItemConfiguration.cs` — add Variant FK.
- **FILE-006**: `service/Api/src/Module/Inventory/Persistence/Configurations/StockReservations/StockReservationConfiguration.cs` — add Variant/StockLocation/Order FKs.
- **FILE-007**: `service/Api/src/Module/Inventory/Persistence/Configurations/StockTransfers/TransferItemConfiguration.cs` — add Variant FK.
- **FILE-008**: `service/Api/src/Module/Inventory/Persistence/Configurations/StockLocations/StockLocationConfiguration.cs` — add Country/State FKs.
- **FILE-009**: `service/Api/src/Module/Customer/Persistence/Configurations/WishedItem.Configuration.cs` — add Variant FK.
- **FILE-010**: `service/Api/src/Module/Customer/Persistence/Configurations/Wishlist.Configuration.cs` — make User FK explicit.
- **FILE-011**: `service/Api/src/Module/Customer/Persistence/Configurations/UserProfile.Configuration.cs` — make User FK explicit.
- **FILE-012**: `AGENTS.md` — drop rule #2 prohibition; document allowed cross-module FKs.
- **FILE-013**: `scripts/check-cross-module-refs.sh` — whitelist-based check replacing fixed baseline.
- **FILE-014**: `Directory.Build.targets` — update `ValidateVerticalSliceIsolation`.
- **FILE-015**: `docs/codebase/ARCHITECTURE.md` and `docs/codebase/CONCERNS.md` — document intentional cross-module FKs.
- **FILE-016**: `service/Api/src/Migrations/Migrations/20260816090623_IntialCreate.cs` and `.Designer.cs` — deleted.
- **FILE-017**: `service/Api/src/Migrations/Migrations/ApplicationDbContextModelSnapshot.cs` — regenerated.
- **FILE-018**: New generated migration file(s) under `service/Api/src/Migrations/Migrations/` — `20260816112407_InitialCreate.cs` + `.Designer.cs` (44 CreateTable, 79 CreateIndex, verified FKs/OnDelete; regenerated after TASK-026 nav additions and TASK-028 constraint edits so the snapshot embeds all inverse navigations and the new column max-lengths/precision: `billing.payment_methods.presentation` varchar(500), `shipping.shipping_methods.presentation` varchar(500), `stock_reservations.reason` varchar(255), `user_profiles.total_spent` numeric(18,2)).
- **FILE-019**: Domain navigation properties added: `Order.cs` (User/BillAddress/ShipAddress/ShippingRate + Shipments/PaymentCaptures/StockReservations), `Shipment.cs` (Order/Address), `ShippingMethodZone.cs` (ShippingMethod), `ShippingMethod.cs` (ShippingMethodZones/Shipments/Orders), `Variant.cs` (StockReservations/TransferItems/WishedItems), `StockReservation.cs` (Variant/StockLocation/Order), `StockTransfer.cs`/TransferItem (Variant/StockTransfer), `WishedItem.cs` (Variant), `PaymentCapture.cs` (Order), `UserProfile.cs` (DefaultBillingAddress/DefaultShippingAddress), `StockLocation.cs` (StockReservations/SourceStockTransfers/DestinationStockTransfers), `OptionValue.cs` (OptionValueVariants), `PaymentMethod.cs` (Orders), `ShippingRate.cs` (Orders), `Address.cs` (BillingOrders/ShippingOrders/Shipments).

## 6. Testing

- **TEST-001**: `dotnet build` (warnings-as-errors) — zero warnings, validates all config edits compile. **PASS** 2026-08-16.
- **TEST-002**: `dotnet test service/Api/tests/Module.UnitTests` + `service/Api/tests/Shared.UnitTests` — regression coverage for configurations/relationships. **Blocked by pre-existing infra**: xunit v3 MTP discovers 0 tests in both projects (exit code 5) independent of this plan's changes.
- **TEST-003**: `dotnet ef migrations has-pending-model-changes` returns "No changes" — proves snapshot == model. **PASS** 2026-08-16.
- **TEST-004**: Manual inspection of the generated migration C# — every FK in TASK-021 present with intended `OnDelete`; `master_variant_id` has no FK/index; no duplicate FK pairs. **PASS** 2026-08-16.
- **TEST-005**: `bash scripts/check-cross-module-refs.sh` exits 0 with the whitelist — enforcement reflects the dropped rule. **PASS** 2026-08-16.
- **TEST-006**: `bash scripts/check-feature-conventions.sh` exits 0 — no feature-file drift introduced. **PASS** 2026-08-16.

## 7. Risks & Assumptions

- **RISK-001**: Existing rows with orphaned `Guid` values in newly-FK'd columns will fail the fresh migration. Mitigation: if the database is seeded/recreated (dev), no backfill needed; for a real DB, run a pre-migration orphan audit (TASK-021 inspection) before applying.
- **RISK-002**: `DeleteBehavior.Restrict` on `Order.UserId`, `Shipment.OrderId`, etc. may surface delete-order violations in existing integration tests. Mitigation: run `dotnet test` (TASK-023) and fix test setup, not delete behavior.
- **RISK-003**: `dotnet ef migrations remove` may fail because the current migration is untracked. Mitigation: fall back to manual snapshot truncation (TASK-019).
- **RISK-004**: Changing implicit conventions (e.g., `Shipment → ShippingMethod` from Cascade to Restrict, `Wishlist → User` Cascade to Restrict) alters runtime delete semantics. Mitigation: documented in CONCERNS.md (TASK-016) and asserted by TASK-021 inspection.
- **ASSUMPTION-001**: The database is recreated from scratch in dev (Aspire-managed), so dropping and regenerating `InitialCreate` is safe.
- **ASSUMPTION-002**: `Wishlist`, `UserProfile`, `Order`, `Shipment`, `ShippingRate` still expose the navigation properties referenced by the explicit `HasOne(x => x.X)` calls; verified during Phase 1. New navs added for the previously-anonymous relationships (see FILE-019).
- **ASSUMPTION-003**: No concurrent schema-affecting work touches `ApplicationDbContextModelSnapshot.cs` during Phase 3.
- **ASSUMPTION-004**: The xunit v3 MTP "Zero tests ran" failure is a pre-existing repository issue, not introduced by this plan; unit-test verification deferred until the test harness is fixed.

## 8. Related Specifications / Further Reading

- `docs/codebase/ARCHITECTURE.md` — module isolation and data-flow documentation updated by this plan.
- `docs/codebase/CONCERNS.md` — prior cross-module violation inventory superseded by the whitelist.
- `docs/codebase/TESTING.md` — testing strategy for EF Core configurations.
- `docs/thesis/05-database-design.md` — intended ERD and schema-organization reference.
- `service/Api/src/Api/DesignTimeDbContextFactory.cs` — design-time context used by the migration commands.
