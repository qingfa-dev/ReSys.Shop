---
goal: Review and correct all response-DTO mappings for Inventory, Billing, and Ordering (admin + storefront API), ensure EF Core queries include the navigation joins each mapper requires, add comprehensive unit tests for both API sides, and fix both SPAs' TypeScript types to match the corrected backend contract
version: 1.0
date_created: 2026-08-18
owner: ngtphat
status: 'Completed'
tags: [refactor, mapping, efcore, tests, frontend, api-contract]
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

An audit of the Inventory, Billing, and Ordering modules (admin + storefront API
features) found three classes of defects:

1. **EF Core query gaps cause NullReferenceExceptions under the relational
   provider.** `OrderMapping.MapToDetailCore` (`Ordering/Features/Admin/Shared/Mappings/Order.Mapping.cs:62-157`)
   dereferences four collections — `Adjustments` (lines 80,92), `PaymentCaptures`
   (118), `Shipments` (134), `LineItems` (150) — with **no null guards**. The
   codebase does **not** use lazy-loading proxies, so un-`Include`d navigations
   are `null` on materialized entities. Eleven Ordering handlers load an `Order`
   with an incomplete include set and then call `MapToDetail`, risking NREs
   (e.g. `ApproveOrder`, `CompleteOrder`, `ResumeOrder`, `UpdateOrderAdmin`,
   `UpdateBillAddress`, `UpdateShipAddress`, `AddOrderLineItem`,
   `UpdateOrderLineItem`, `UpdateOrderShippingMethod`, `CancelOrderAdmin`,
   `CreateOrderFromCart`). Existing tests use the EF **InMemory** provider, which
   does NOT reproduce relational null-nav behavior, so the bug is masked.

2. **Mapping gaps leave DTO properties unpopulated.** Examples: Billing
   `PaymentMethodName` never populated (nav not included); `PaymentStatus` left
   default null in `MapToDetail`/`MapToListItem`; SetupIntent response has only
   `ClientSecret` set; `ConfirmPayment.Response.Message` never assigned;
   `GetPaymentStatus` uses a partial manual initializer; Ordering
   `LineItemResponse.OrderId`/`Product*` never populated on the admin list
   endpoints; `GetOrderTracking` never populates `DeliveryExceptionAt` /
   `EstimatedDeliveryAt`.

3. **Type mismatches in both SPAs** versus the backend contract (e.g. Admin
   `partiallyFulfilled` typed `boolean` but backend is `int`;
   `StockTransferDetail` item omits `id`; Admin `OrderDetail`/`LineItem` omit
   backend fields; Store `PaymentIntent` omits `checkoutUrl`; Store
   `CartReservationStatus` omits `modifiedAtUtc`).

This plan fixes the root causes (correct includes, complete mappings), adds unit
tests that would have caught them, and aligns both SPAs' types.

## 1. Requirements & Constraints

- **REQ-001**: Every response DTO mapper must fully populate every non-optional DTO property from the source entity/aggregate (no properties silently left at default/null unless the backend type declares them nullable by design).
- **REQ-002**: Every EF Core query that materializes an entity then passes it to a mapper reading navigation properties must `.Include()`/`.ThenInclude()` those navigations. No mapper may rely on lazy loading (not enabled).
- **REQ-003**: `MapToDetailCore` must be made resilient: the four Ordering query-returning handlers must load all required navigations; the mapper may also guard against null collections defensively.
- **REQ-004**: New unit tests must cover every previously-untested handler in Inventory, Billing, and Ordering (admin + storefront), and must assert mapped DTO field values (not just success/failure), so missing includes/mappings are caught.
- **REQ-005**: Both SPAs' TypeScript interfaces must match the corrected backend DTOs field-for-field (names, optionality, types).
- **SEC-001**: No new secrets, PII, or payment data logging; tests use the existing InMemory `ApplicationDbContext` pattern with `AdditionalConfigurationsAssemblies`.
- **CON-001**: `TreatWarningsAsErrors=true` — any warning fails the build; all new code compiles clean.
- **CON-002**: Vertical-slice feature file structure preserved; changes stay in their feature files; tests live under `service/Api/tests/Module.UnitTests/{Module}/...`.
- **CON-003**: Do NOT change JSON wire format (`JsonStringEnumConverter` stays); enum member names serialize as PascalCase strings.
- **CON-004**: Test runner: `dotnet test --filter` does NOT work (xunit v3 MTP). Run via `cd service/Api/tests/Module.UnitTests/bin/Debug/net10.0 && ./Module.UnitTests -class "<FullyQualifiedClassName>"` after `dotnet build service/Api/src/Api/Api.csproj`.
- **CON-005**: The InMemory provider keeps navigations populated even without `.Include()`, so handler tests must be written to assert DTO field content and, where feasible, to detect missing-include bugs (e.g. via a relational-backed integration test or by asserting the query's include behavior).
- **CON-006**: Do not modify the domain entities or their EF configurations except where a mapping genuinely needs a newly-exposed navigation; prefer fixing the query include over altering the entity.
- **GUD-001**: Follow the existing handler-test pattern in `Ordering/Features/Admin/Orders/Approve/ApproveOrderTests.cs` (ctor builds `ApplicationDbContext` + mocks; `[Trait]` attributes; `IDisposable`).
- **GUD-002**: SPA type changes mirror the backend DTO records exactly; add a type-comment header noting the source backend record.
- **PAT-001**: Mapping: `entity.Navigation?.Prop` null-safe where the nav is genuinely optional; otherwise the query must `Include` it.
- **PAT-002**: EF query: build a helper `OrderQuery.Includes(Order)` extension or per-handler explicit includes; prefer `AsNoTracking()` for reads.
- **PAT-003**: Tests assert mapped values (e.g. `result.Value.Payments.Should().HaveCount(1)`, `result.Value.Shipments[0].ShippingMethodName.Should().Be(...)`).

## 2. Implementation Steps

### Implementation Phase 1

- GOAL-001: Fix the Ordering EF Core include gaps (NRE risk) and add regression tests for the query-returning handlers.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `Ordering/Features/Admin/Shared/Extensions/OrderQuery.cs` (or add to an existing query helper file) with `public static IQueryable<Order> IncludeOrderDetail(this IQueryable<Order> q)` that chains `.Include(o => o.LineItems).Include(o => o.Adjustments).Include(o => o.PaymentCaptures).Include(o => o.Shipments).ThenInclude(s => s.ShippingMethod)`. | ✅ | 2026-08-18 |
| TASK-002 | Update the 9 no-include `MapToDetail` handlers to use the include helper before `FirstOrDefaultAsync`: `ApproveOrder.cs:21`, `CompleteOrder.cs:21`, `ResumeOrder.cs:28`, `UpdateOrderAdmin.cs:21`, `UpdateOrderBillAddress.cs:21`, `UpdateOrderShipAddress.cs:21`, `AddOrderLineItem.cs:21`, `UpdateOrderLineItem.cs:22`. | ✅ | 2026-08-18 |
| TASK-003 | Update `UpdateOrderShippingMethod.cs:22-25` to also include `PaymentCaptures` and `Shipments` (+`ThenInclude(ShippingMethod)`). | ✅ | 2026-08-18 |
| TASK-004 | Update `CancelOrderAdmin.cs:34-38` to also include `Adjustments` and add `ThenInclude(ShippingMethod)` on Shipments. | ✅ | 2026-08-18 |
| TASK-005 | Update `CreateOrderFromCart.cs:26-30` (Storefront checkout) to also include `PaymentCaptures` and `Shipments` (+`ThenInclude(ShippingMethod)`). | ✅ | 2026-08-18 |
| TASK-006 | Update `CreateCart.cs:28-30` existing-cart path to also include `Adjustments`. | ✅ | 2026-08-18 |
| TASK-007 | Add defensive null guards to `MapToDetailCore` (`Order.Mapping.cs:62-157`): wrap the four collection dereferences (`Adjustments`, `PaymentCaptures`, `Shipments`, `LineItems`) with `entity.Adjustments ?? []` etc., so a future missing include fails softly (null collection → empty lists) instead of NRE. | ✅ | 2026-08-18 |
| TASK-008 | Add handler tests (InMemory) asserting mapped DTO content for: `CompleteOrder`, `ResumeOrder` (extend existing), `UpdateOrderAdmin`, `UpdateBillAddress`, `UpdateShipAddress`, `AddOrderLineItem`, `UpdateOrderLineItem`, `UpdateOrderShippingMethod` — each seeds an order with LineItems+Adjustments+PaymentCaptures+Shipments and asserts `result.Value.Payments/Shipments/Adjustments/LineItems` are populated and correct. | ✅ | 2026-08-18 |
| TASK-009 | Build + run the Ordering unit tests (`./Module.UnitTests -class "Module.UnitTests.Ordering.Features.Admin.Orders.*"` for the touched handlers). | ✅ | 2026-08-18 |

### Implementation Phase 2

- GOAL-002: Fix Ordering mapping gaps (`LineItemResponse`, `GetOrderTracking`) and Storefront line-item enrichment.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-010 | `Order.Mapping.MapToLineItemResponse<T>` (`Order.Mapping.cs:210-223`): populate `OrderId` (from `li.OrderId`) and, when a lookup is available, `ProductId`/`ProductName`/`ProductImageUrl` (the lookup overload already does this — ensure the plain overload sets `OrderId`). | ✅ | 2026-08-18 |
| TASK-011 | `GetOrderLineItems.cs:35`: use the lookup-enriched overload (`MapToLineItemResponse<Response>(itemLookup)`) so Product fields are populated on the admin line-item list endpoint, matching `GetOrderById`. Build the itemLookup via `ProductLookupFactory.BuildAsync` like `GetOrderById.cs:35-40`. | ✅ | 2026-08-18 |
| TASK-012 | `GetOrderLineItemById.cs:30`: apply the same lookup enrichment. | ✅ | 2026-08-18 |
| TASK-013 | `GetOrderTracking.cs:20-48`: populate `EstimatedDeliveryAt` and `DeliveryExceptionAt` from the already-queried `shipment` (`shipment.EstimatedDeliveryAtUtc` / a delivery-exception timestamp field if present, else leave nullable). Remove the unused `shipment` query if no field feeds it, or wire it in. | ✅ | 2026-08-18 |
| TASK-014 | Add mapping tests: `LineItemResponse` OrderId/Product fields populated via lookup; `GetOrderTracking` returns EstimatedDeliveryAt when the shipment has one. | ✅ | 2026-08-18 |
| TASK-015 | Build + run the Ordering unit tests for the touched handlers. | ✅ | 2026-08-18 |

### Implementation Phase 3

- GOAL-003: Fix Billing mapping/query gaps (PaymentMethodName include, PaymentStatus, SetupIntent, ConfirmPayment.Message, GetPaymentStatus).

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-016 | Add `.Include(p => p.PaymentMethod)` to the Billing queries that feed `PaymentModelMapping.MapToDetail` / `MapToListItem` (GetPaymentById, GetPagedPayments) and to `CapturePayment`/`RefundPayment`/`VoidPayment` handlers when they read `payment.PaymentMethod`. | ✅ | 2026-08-18 |
| TASK-017 | `Payment.Mapping.MapToDetail`/`MapToListItem` (`Payment.Mapping.cs:18-50`): populate `PaymentStatus = payment.PaymentStatus` (currently left default null). | ✅ | 2026-08-18 |
| TASK-018 | `Storefront.Payment.Mapping.MapToStoreDetail<T>(PaymentGatewayResponse)` (`Storefront.Payment.Mapping.cs:10-16`): this overload only sets `ClientSecret`. Decide with the plan author whether the SetupIntent response should also carry the other fields (Id/Amount/Currency/OrderId/PaymentMethodId/State/PaymentStatus) from the corresponding `PaymentCapture`; if the gateway response has no capture, keep it minimal but document. Prefer populating from the capture if one exists. | ✅ | 2026-08-18 |
| TASK-019 | `ConfirmPayment.cs`: assign `Message` on the returned `Response` (e.g. `"Payment confirmed"` / `"Payment already completed"` / a failure message) in each of the three return paths (`:40,:49,:53`), instead of leaving it default `""`. | ✅ | 2026-08-18 |
| TASK-020 | `GetPaymentStatus.cs:38-46`: use the full `MapToStoreDetail<Response>` mapping (or extend the manual initializer) so `PaymentMethodId`, `ClientSecret`, `CheckoutUrl`, `ResponseCode`, `PaymentStatus`, `CreatedAtUtc`, `ModifiedAtUtc` are populated, not left default. Keep `IsCompleted`. | ✅ | 2026-08-18 |
| TASK-021 | Add Billing mapping/handler tests: `MapToDetail`/`MapToListItem` populate `PaymentStatus` + `PaymentMethodName` (with a PaymentMethod); ConfirmPayment sets `Message`; GetPaymentStatus populates the full response; SetupIntent response shape. | ✅ | 2026-08-18 |
| TASK-022 | Build + run the Billing unit tests. | ✅ | 2026-08-18 |

### Implementation Phase 4

- GOAL-004: Fix Inventory mapping/query gaps (Storefront reserve raw-entity, GetCartReservations, and any query-include needs).

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-023 | `ReserveStockReservation.Endpoint.cs:29` (Storefront) returns the raw `StockReservation` entity with null navs. Map it to a DTO instead (or add the missing `Include` for any nav the SPA reads). Confirm the SPA's `CartReservation` type and align. | ✅ | 2026-08-18 |
| TASK-024 | `GetCartReservations.Response.CartReservationStatus` already complete (audit). Add `modifiedAtUtc` mapping if the SPA needs it (align with Store `availability.ts`). | ✅ | 2026-08-18 |
| TASK-025 | Verify `GetStockTransferPagedOrAll` `TotalItems` (SQL projection) and `GetStockTransferById` `TransferItems` include are correct; add handler tests asserting `Items`/`TotalItems` are populated. | ✅ | 2026-08-18 |
| TASK-026 | Add handler tests for previously-untested Inventory handlers (audit list): `GetStockItemById`, `UpdateStockItem`, `DeleteStockItem`, `ImportStockItems` (handler), `GetPagedStockMovements`, `GetStockMovementById`, `GetPagedStockReservations`, `GetStockReservationById`, `CancelStockReservation`, `GetStockTransferById`, `GetStockTransferPagedOrAll`, `CreateStockTransfer`, and the Storefront `GetCartReservations`/`Reserve`/`Release`/`GetStockAvailability`. Expand `BulkAdjustStockItems` to cover success/not-found/multi-item. | ✅ | 2026-08-18 |
| TASK-027 | Build + run the Inventory unit tests. | ✅ | 2026-08-18 |

### Implementation Phase 5

- GOAL-005: Add the remaining Ordering handler tests (previously-untested handlers and mapping-content assertions).

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-028 | Add handler tests for: `GetPagedOrders`, `GetOrderLineItems`, `GetOrderLineItemById` (assert OrderId/Product fields), `CompleteOrder`, `CreateOrder` (admin), `UpdateOrderAdmin`, `UpdateBillAddress`, `UpdateShipAddress`, `AddOrderLineItem`, `UpdateOrderLineItem`, `RemoveOrderLineItem`, `GetOrderTracking` (assert EstimatedDeliveryAt), `GetCartForShipping`, `ValidateCheckout`, `RegressCheckoutState` (handler). | ✅ | 2026-08-18 |
| TASK-029 | Add a `MapToDetailWithLookup` mapping test asserting `LineItems` are populated with Product fields via the lookup overload. | ✅ | 2026-08-18 |
| TASK-030 | Add a relational-backed integration test (or a targeted test) asserting the include-helper query loads all four collections — if Testcontainers integration is unavailable, assert via a SQL-translatable query or document the InMemory limitation. | ✅ | 2026-08-18 |
| TASK-031 | Build + run the full Ordering unit tests. | ✅ | 2026-08-18 |

### Implementation Phase 6

- GOAL-006: Fix the Admin SPA types to match the corrected backend contract.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-032 | `app/Admin/src/features/inventory/types/stockItem.ts:87`: change `RestockResultResponse.partiallyFulfilled` from `boolean` to `number` (backend `PartiallyFulfilled: int`). | ✅ | 2026-08-18 |
| TASK-033 | `app/Admin/src/features/inventory/types/stockTransfer.ts:28-42`: add `id` to the transfer item type; make `state` optional to match `TransferState?`. Update `StockTransferDetail.vue:376` `data-key` to `id`. | ✅ | 2026-08-18 |
| TASK-034 | `app/Admin/src/features/ordering/types/order.ts:31-67`: add missing `shippingCalculation` (`ShippingCalculationSummary?`) and `adjustments` (`AdjustmentSummary[]`) to `OrderDetail`. Add the `AdjustmentSummary`/`ShippingCalculationSummary` interfaces. | ✅ | 2026-08-18 |
| TASK-035 | `app/Admin/src/features/ordering/types/order.ts:69-78`: add `orderId`, `productId?`, `productName?`, `productImageUrl?` to `LineItem`; make `variantId` nullable to match `VariantId?`. | ✅ | 2026-08-18 |
| TASK-036 | `app/Admin/src/features/ordering/types/order.ts:193`: change `ShipmentSummary.trackingNumber` from `string \| null` to `string` (backend non-null). | ✅ | 2026-08-18 |
| TASK-037 | Run `cd app/Admin && pnpm run lint && pnpm run type-check && pnpm run test:unit`; fix any failures and add a type-level test if the codebase has one. | ✅ | 2026-08-18 |

### Implementation Phase 7

- GOAL-007: Fix the Storefront SPA types to match the corrected backend contract.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-038 | `app/Store/src/features/ordering/types/order.ts:19-31`: add `orderId` to `OrderLineItem`; `:89`: change `ShipmentSummary.trackingNumber` to `string`. | ✅ | 2026-08-18 |
| TASK-039 | `app/Store/src/features/payment/types/payment.ts:47-60`: add `settings?: Record<string, string> | null` to `PaymentMethod`; `:63-75`: add `checkoutUrl?: string | null` to `PaymentIntent`. | ✅ | 2026-08-18 |
| TASK-040 | `app/Store/src/features/inventory/types/availability.ts:36-47`: add `modifiedAtUtc?: string | null` to `CartReservationStatus`; `:11-18`: remove `orderId`/`reason` from `ReserveStockRequest` (backend request doesn't accept them) or align with the backend request type. | ✅ | 2026-08-18 |
| TASK-041 | Run `cd app/Store && pnpm run lint && pnpm run type-check && pnpm run test:unit`; fix any failures. | ✅ | 2026-08-18 |

### Implementation Phase 8

- GOAL-008: Full verification across backend and both SPAs.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-042 | `dotnet build service/Api/src/Api/Api.csproj` (0 warnings/0 errors) and run the full `Module.UnitTests` suite (expect only the 3 pre-existing `OrderStatusValueConverterTests` failures). | ✅ | 2026-08-18 |
| TASK-043 | Run `bash scripts/check-feature-conventions.sh`; confirm no new drift. | ✅ | 2026-08-18 |
| TASK-044 | Update this plan's task table (Completed/Date) and set front-matter `status` to `Completed` once all phases pass. | ✅ | 2026-08-18 |

## 3. Alternatives

- **ALT-001**: Enable EF Core lazy-loading proxies to avoid manual `.Include()`. Rejected — the codebase explicitly does not use them, and adding them risks N+1 queries and proxy-vs-entity type issues across all modules.
- **ALT-002**: Make `MapToDetailCore` only null-guard without fixing the queries. Rejected — null-guarding hides the data-loss (empty Payments/Shipments in responses) instead of returning correct data; both the include fix and defensive guards are required.
- **ALT-003**: Fix only the SPAs and skip backend mapping/query changes. Rejected — the backend is the source of truth; SPA-only fixes would mask missing data.

## 4. Dependencies

- **DEP-001**: Existing domain entities/navigations for Ordering (`Order.LineItems/Adjustments/PaymentCaptures/Shipments(+ShippingMethod)`), Billing (`PaymentCapture.PaymentMethod`), Inventory (`StockItem.StockLocation`, `StockTransfer.TransferItems`) — all already exist; no schema change.
- **DEP-002**: `ProductLookupFactory.BuildAsync` (`Ordering/Features/Storefront/Shared/Services/`) — used to enrich admin line-item Product fields.
- **DEP-003**: EF InMemory `ApplicationDbContext` test harness (`ApplicationDbContext.AdditionalConfigurationsAssemblies`) — used by all new handler tests.
- **DEP-004**: `dotnet-ef` CLI + migration project — unchanged; no migration expected (mapping/query/type-only changes; no entity property additions unless CON-006 is invoked).

## 5. Files

- **FILE-001**: `service/Api/src/Module/Ordering/Features/Admin/Shared/Mappings/Order.Mapping.cs` — null-guard `MapToDetailCore`; `MapToLineItemResponse` OrderId.
- **FILE-002**: `service/Api/src/Module/Ordering/Features/Admin/Shared/Extensions/OrderQuery.cs` — new `IncludeOrderDetail` helper.
- **FILE-003**: Ordering query-returning handlers (Approve, Complete, Resume, UpdateOrderAdmin, UpdateBillAddress, UpdateShipAddress, AddOrderLineItem, UpdateOrderLineItem, UpdateOrderShippingMethod, CancelOrderAdmin) — include fix.
- **FILE-004**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs` + `Cart/CreateCart/CreateCart.cs` — include fix.
- **FILE-005**: `service/Api/src/Module/Ordering/Features/Admin/Orders/Get/LineItems/GetOrderLineItems.cs` + `Get/LineItemById/GetOrderLineItemById.cs` — lookup enrichment.
- **FILE-006**: `service/Api/src/Module/Ordering/Features/Storefront/Orders/GetTracking/GetOrderTracking.cs` — EstimatedDeliveryAt/DeliveryExceptionAt.
- **FILE-007**: `service/Api/src/Module/Billing/Features/Admin/Shared/Mappings/Payment.Mapping.cs` + `Storefront/Shared/Mappings/Storefront.Payment.Mapping.cs` — PaymentStatus/PaymentMethodName/SetupIntent.
- **FILE-008**: Billing handlers: `Admin/Payments/{Get/ById,Get/Paged,Capture,Refund,Void}`, `Storefront/Payment/{Confirm,Status,SetupIntent}` — includes + Message/initializer fixes.
- **FILE-009**: `service/Api/src/Module/Inventory/Features/Storefront/StockReservations/*` — Reserve DTO mapping, GetCartReservations modifiedAtUtc.
- **FILE-010**: `app/Admin/src/features/inventory/types/stockItem.ts` + `stockTransfer.ts` + `views/StockTransferDetail.vue`.
- **FILE-011**: `app/Admin/src/features/ordering/types/order.ts`.
- **FILE-012**: `app/Store/src/features/ordering/types/order.ts` + `payment/types/payment.ts` + `inventory/types/availability.ts`.
- **FILE-013**: `service/Api/tests/Module.UnitTests/{Ordering,Billing,Inventory}/...` — new handler + mapping tests.

## 6. Testing

- **TEST-001**: Ordering handler tests assert `MapToDetail` responses populate `LineItems`/`Adjustments`/`Payments`/`Shipments` (catches missing includes).
- **TEST-002**: Ordering `MapToDetailWithLookup` + `GetOrderLineItems`/`GetOrderLineItemById` assert Product fields are populated.
- **TEST-003**: Billing mapping tests assert `PaymentStatus`/`PaymentMethodName`; ConfirmPayment `Message`; GetPaymentStatus full response.
- **TEST-004**: Inventory handler tests for the 12+ untested handlers + expanded `BulkAdjustStockItems`.
- **TEST-005**: Admin SPA type-check + unit tests after type corrections.
- **TEST-006**: Storefront SPA type-check + unit tests after type corrections.
- **TEST-007**: Full `Module.UnitTests` suite + `check-feature-conventions.sh` + backend build (0 warnings/0 errors).

## 7. Risks & Assumptions

- **RISK-001**: The InMemory provider may not catch missing-include NREs; mitigation is defensive null-guards in the mapper plus content-asserting handler tests (see CON-005 / TASK-030).
- **RISK-002**: Populating new DTO fields (e.g. `PaymentMethodName`, `EstimatedDeliveryAt`) may surface in SPAs that previously showed defaults — verify SPA rendering still works (Phases 6-7).
- **RISK-003**: Changing `partiallyFulfilled` from `boolean` to `number` in the Admin SPA may break a view that treated it as boolean — search all usages in TASK-032.
- **ASSUMPTION-001**: All three modules use one shared `ApplicationDbContext`; navigations across modules (Order→Shipment, Order→PaymentCapture) resolve via the shared model (confirmed by prior cross-module FK work).
- **ASSUMPTION-002**: No entity property additions are required; all fixes are query-include, mapping, or SPA-type changes.
- **ASSUMPTION-003**: The 3 pre-existing `OrderStatusValueConverterTests` failures are unrelated and out of scope.

## 8. Related Specifications / Further Reading

- [Codebase architecture](./codebase/ARCHITECTURE.md)
- [Domain principles](./../.harness/principles.yml)
- [Enforcement rules](./../.harness/enforcement.yml)
- [Order enrichment design (timeline statuses)](../docs/superpowers/specs/2026-08-16-order-details-enrichment-design.md)
- [Cross-module state sync design](../docs/superpowers/specs/2026-08-16-order-cross-module-state-sync-design.md)
- [EF Core migration guide](service/Api/src/Migrations/GUIDE.yaml)