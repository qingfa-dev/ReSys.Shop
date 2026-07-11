---
goal: Add unit tests for all 10 mapping classes currently missing test coverage
version: 1.0
date_created: 2026-07-11
status: Planned
tags: test, mappings, unit-tests
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Add 10 new test files in `Module.UnitTests/` covering every mapping method in the 10 untested mapping classes. Follow the established pattern at `Ordering/Features/Admin/Orders/Shared/Mappings/Order.Mapping.Tests.cs`: `[Trait("Category", "Unit")]`, `[Fact]`, FluentAssertions `.Should()`, namespace `Module.UnitTests.{Area}.Features...`.

## 1. Requirements & Constraints

- **REQ-001**: Every public mapping method must have at least one `[Fact]` verifying the happy path maps all properties correctly.
- **REQ-002**: Every mapping method with nullable/optional source fields must have an additional `[Fact]` testing null/empty edge cases.
- **REQ-003**: Every `MapToDomain<T>` returning `Result<T>` must test both `IsSuccess` and `IsFailure` paths.
- **REQ-004**: Every `MapToDetail<T>` / `MapToListItem<T>` must use FluentAssertions to assert each mapped property individually.
- **REQ-005**: Test files must mirror source directory structure under `Module.UnitTests/`.
- **REQ-006**: All tests must use `[Trait("Category", "Unit")]` and module-level `[Trait("Module", "...")]`.
- **CON-001**: No external dependencies — tests create domain entities via domain `Create`/`Method.Create` factories, never mocked.
- **CON-002**: Must pass `dotnet test Module.UnitTests` (excluding pre-existing Stripe test failures).
- **PAT-001**: Follow `Ordering/Features/Admin/Orders/Shared/Mappings/Order.Mapping.Tests.cs` as reference — static `CreateEntity()` helper with `Action<Entity>? configure` callback.

## 2. Implementation Steps

### Phase 1 — Catalog Storefront Mappings

- GOAL-001: Test `ProductStoreMapping`, `OptionTypeStoreMapping`, `TaxonomyStoreMapping` — all storefront-facing entity-to-response mappers with nested sub-mappings.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `Module.UnitTests/Catalog/Features/Storefront/Products/Shared/Mappings/ProductStore.Mapping.Tests.cs`. Test `MapToStoreDetail<T>`: create `Product` with 1 master variant + 1 child variant + 2 images + 1 classification + 1 price. Assert all 10+ properties of `StoreProductDetailResponse`. Test `MapToStoreListItem<T>`: assert `MinPrice`, `Currency`, `ThumbnailUrl`, `VariantsCount`. Test `MapToStoreVariant`: assert `Id`, `Sku`, `IsMaster`, `Price`, `Currency`, `Images`. Test `MapToStoreImage`: assert `Id`, `Url`, `Alt`, `Position`, `ContentType`. | | |
| TASK-002 | Create `Module.UnitTests/Catalog/Features/Storefront/OptionTypes/Shared/Mappings/OptionTypeStore.Mapping.Tests.cs`. Test `MapToStoreResponse<T>`: create `OptionType` with 2 `OptionValue`s, assert `Id`, `Name`, `Presentation`, `Position`, `Values` count and properties. Test `MapToStoreValue`: assert `Id`, `Name`, `Presentation`, `Position`. | | |
| TASK-003 | Create `Module.UnitTests/Catalog/Features/Storefront/Taxonomies/Shared/Mappings/TaxonomyStore.Mapping.Tests.cs`. Test `MapToStoreTree<T>`: create `Taxonomy` with a nested `Taxon` tree (parent + 2 children), assert `Nodes` builds recursive tree structure with correct `Id`, `Name`, `Permalink`, `Depth`, `Children`. | | |

### Phase 2 — Inventory Admin Mappings

- GOAL-002: Test `StockTransferMapping` and `StockReservationMapping` — both have Model (detail/list) and Domain (request-to-entity) sides.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004 | Create `Module.UnitTests/Inventory/Features/Admin/StockTransfers/Shared/Mappings/StockTransfer.Mapping.Tests.cs`. Test `MapToDetail<T>`: create `StockTransfer` via `StockTransferExtensions.Create` with 2 `TransferItem`s, assert all detail properties + `Items` collection. Test `MapToListItem<T>`: assert list properties + `TotalItems`. Test `MapToDomain<T>` (create): create `StockTransferRequest`, call `MapToDomain`, assert `IsSuccess`, verify entity properties match request. | | |
| TASK-005 | Create `Module.UnitTests/Inventory/Features/Admin/StockReservations/Shared/Mappings/StockReservation.Mapping.Tests.cs`. Test `MapToDetail<T>`: create `StockReservation` via `StockReservationMethod.Reserve`, assert all 10 properties. Test `MapToListItem<T>`: assert list properties. Test `MapToDomain<T>`: create `StockReservationRequest`, call `MapToDomain`, assert `IsSuccess`, verify entity fields. Test `MapToDomain<T>` failure: pass invalid quantity (e.g., 0) and assert `IsFailure`. | | |

### Phase 3 — Ordering Storefront Cart Mapping

- GOAL-003: Test `CartMapping` — note `MapToDetail<T>` is currently a stub returning default values; test the stub as-is and flag for enrichment separately.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | Create `Module.UnitTests/Ordering/Features/Storefront/Cart/Shared/Mappings/Cart.Mapping.Tests.cs`. Test `MapToDetail<T>` (stub): create `Order` via `OrderExtensions.Create`, call `MapToDetail<CartDetailResponse>`, verify all properties are default/zero (current stub behavior). Add `[Fact(Skip = "Stub — enrich CartMapping first")]` noting `MapToDetail` is incomplete. | | |

### Phase 4 — Payment Admin Mappings

- GOAL-004: Test `PaymentMethodMapping` — Model (detail, list) and Domain (create, update, patch) with 5 mapping methods.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-007 | Create `Module.UnitTests/Payment/Features/Admin/PaymentMethods/Shared/Mappings/PaymentMethod.Mapping.Tests.cs`. Test `MapToDetail<T>`: create `PaymentMethod` via `PaymentMethodMethod.Create`, assert all 13 properties including `Settings`, `Preferences`. Test `MapToListItem<T>`: assert list properties. Test `MapToDomain<T>` (create): create `PaymentMethodRequest`, assert `IsSuccess`, verify entity fields match request. Test `MapToDomain<T>` (update): create entity, create `PaymentMethodRequest`, call `request.MapToDomain(entity)`, assert `IsSuccess`, verify entity updated. Test `MapUpdateToDomain<T>`: create entity, create `PaymentMethodUpdateRequest` with partial fields, assert method applies only provided fields. | | |

### Phase 5 — Profile Store Wishlist Mapping

- GOAL-005: Test `WishlistMapping` — Model mappings only (Domain side is empty).

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-008 | Create `Module.UnitTests/Profile/Features/Store/Wishlists/Shared/Mappings/Wishlist.Mapping.Tests.cs`. Test `MapToDetail<T>`: create `Wishlist` via `WishlistExtensions.Create`, add 2 `WishedItem`s via `wishlist.AddItem`, assert all properties + `WishedItems` collection (ordered by `CreatedAtUtc` desc). Test `MapToListItem<T>`: assert list properties. Test `MapToSimple<T>`: assert detail properties without `WishedItems`. Test null `Name` edge case. | | |

### Phase 6 — Shipping Admin Mappings

- GOAL-006: Test admin `ShippingMethodMapping` and `ShippingRateMapping` — both have Model (detail, list) and Domain (create, update, patch) sides.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Create `Module.UnitTests/Shipping/Features/Admin/ShippingMethods/Shared/Mappings/ShippingMethod.Mapping.Tests.cs`. Test `MapToDetail<T>`: create `ShippingMethod` via `ShippingMethodExtensions.Create`, assert all 13+ properties. Test `MapToListItem<T>`: assert list properties. Test `MapToDomain<T>` (create): create `ShippingMethodRequest`, assert `IsSuccess`, verify entity properties. Test `MapToDomain<T>` (update): create entity, create request, call `request.MapToDomain(entity)`, assert `IsSuccess`. Test `MapUpdateToDomain<T>`: patch with partial fields. | | |
| TASK-010 | Create `Module.UnitTests/Shipping/Features/Admin/ShippingRates/Shared/Mappings/ShippingRate.Mapping.Tests.cs`. Test `MapToDetail<T>`: create `ShippingRate` via `ShippingRateExtensions.Create`, assert all 12 properties including `DeliveryRange`. Test `MapToListItem<T>`: assert list properties. Test `MapToDomain<T>` (create): create `ShippingRateRequest`, assert `IsSuccess`. Test `MapToDomain<T>` (update): create entity, create request, call `request.MapToDomain(entity)`, assert `IsSuccess`. Test `MapUpdateToDomain<T>`: patch with partial fields. | | |

## 3. Alternatives

- **ALT-001**: Use AutoFixture to auto-generate test data. Rejected — all existing mapping tests use manual factory helpers, keeping consistency.
- **ALT-002**: Merge Model and Domain tests into a single file per entity. Already the convention (e.g., `Order.Mapping.Tests.cs` covers both) — followed here.
- **ALT-003**: Skip `CartMapping` since it's a stub. Rejected — test the contract as-is so the stub doesn't regress.

## 4. Dependencies

- **DEP-001**: TASK-006 (`CartMapping` stub test) must be skipped (`[Fact(Skip)]`) until `CartMapping.MapToDetail<T>` is enriched with real property mapping.
- **DEP-002**: No cross-phase dependencies — all 10 tasks are independent and can run in parallel.
- **DEP-003**: Must run against a clean `dotnet build` — verify source mapping classes compile before writing tests.

## 5. Files

### New test files (10 files)

- **FILE-001**: `Module.UnitTests/Catalog/Features/Storefront/Products/Shared/Mappings/ProductStore.Mapping.Tests.cs`
- **FILE-002**: `Module.UnitTests/Catalog/Features/Storefront/OptionTypes/Shared/Mappings/OptionTypeStore.Mapping.Tests.cs`
- **FILE-003**: `Module.UnitTests/Catalog/Features/Storefront/Taxonomies/Shared/Mappings/TaxonomyStore.Mapping.Tests.cs`
- **FILE-004**: `Module.UnitTests/Inventory/Features/Admin/StockTransfers/Shared/Mappings/StockTransfer.Mapping.Tests.cs`
- **FILE-005**: `Module.UnitTests/Inventory/Features/Admin/StockReservations/Shared/Mappings/StockReservation.Mapping.Tests.cs`
- **FILE-006**: `Module.UnitTests/Ordering/Features/Storefront/Cart/Shared/Mappings/Cart.Mapping.Tests.cs`
- **FILE-007**: `Module.UnitTests/Payment/Features/Admin/PaymentMethods/Shared/Mappings/PaymentMethod.Mapping.Tests.cs`
- **FILE-008**: `Module.UnitTests/Profile/Features/Store/Wishlists/Shared/Mappings/Wishlist.Mapping.Tests.cs`
- **FILE-009**: `Module.UnitTests/Shipping/Features/Admin/ShippingMethods/Shared/Mappings/ShippingMethod.Mapping.Tests.cs`
- **FILE-010**: `Module.UnitTests/Shipping/Features/Admin/ShippingRates/Shared/Mappings/ShippingRate.Mapping.Tests.cs`

### Reference files

- **FILE-011**: `Module.UnitTests/Ordering/Features/Admin/Orders/Shared/Mappings/Order.Mapping.Tests.cs` — pattern reference (factory helper, FluentAssertions, Trait attributes)

## 6. Testing

- **TEST-001**: `dotnet test service/Api/tests/Module.UnitTests --filter "Category=Unit"` — all new tests pass.
- **TEST-002**: `dotnet build service/Api/src/Api/Api.csproj` — 0 warnings, 0 errors (source mappings unchanged).

## 7. Risks & Assumptions

- **RISK-001**: `CartMapping.MapToDetail<T>` is a stub (returns default values). Tests will verify stub behavior, then be skipped/enriched when the mapping is fixed.
- **RISK-002**: Some mapping methods (e.g., `TaxonomyStoreMapping.BuildTree`) are `private` — tested indirectly through the public `MapToStoreTree<T>`.
- **ASSUMPTION-001**: All domain entity `Create` factory methods (e.g., `StockTransferExtensions.Create`, `WishlistExtensions.Create`) accept sufficient parameters for a realistic test entity.
- **ASSUMPTION-002**: Domain entities are disposable in-memory — no database required for mapping unit tests.

## 8. Related Specifications / Further Reading

- `service/Api/tests/Module.UnitTests/Ordering/Features/Admin/Orders/Shared/Mappings/Order.Mapping.Tests.cs` — reference pattern
- `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Prices/Shared/Mappings/Price.Mapping.Tests.cs` — simpler reference pattern
- `docs/codebase/TESTING.md` — testing strategy
