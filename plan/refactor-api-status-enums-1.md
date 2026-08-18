---
goal: Type all status/state fields in API request/response models as domain enums (keeping JSON string serialization), fix display mapping in both SPAs, wire up payment/shipment action buttons, and rebuild the EF Core migration baseline
version: 1.1
date_created: 2026-08-17
last_updated: 2026-08-18
owner: ngtphat
status: 'Completed'
tags: [refactor, enum, api-contract, frontend, actions, migrations]
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

The API currently exposes several status/state fields in request/response DTOs as
plain `string` even though the domain layer already models them as enums (e.g.
`PaymentRecordState`, `TransferState`, `ReservationState`, `VariantImageType`,
`EmbeddingStatus`, `OrderStatus`). A few statuses have no enum at all
(`OrderTimelineEvent.Type`, dashboard `ActivityItemData`, low-stock status) and
need small new enums. The JSON wire format is and stays **string-based**
(global `JsonStringEnumConverter` in `Program.cs:31-33` must remain unchanged),
so this is a type-safety and UI-correctness refactor, not a contract break —
enum member names already match the string values emitted today.

In parallel, the Admin and Storefront SPAs get their status display mapping
fixed (typed unions + label/severity maps) and get **action buttons** wired to
the existing backend endpoints: payment Capture/Refund/Void and shipment
Mark Shipped / Mark Delivered in Admin; Pay-now and Cancel-order in the
Storefront. Backend endpoints already exist; only UI wiring + typings change.

Scope decision (documented): fields backed by a real domain enum are converted.
Free-text audit fields (`PaymentStatus` gateway text, `StockMovement`
Action/Reason/OriginatorType, `AdjustmentSummary.SourceType`,
`ShippingMethod.CalculatorType`) and the `TaxonRule` snake_case `EnumMember`
contract are **kept as strings** to avoid corrupting free-form/provider data or
the rule editor wire contract.

After the enum typing is complete, the EF Core migration baseline is rebuilt:
the existing single migration (`20260816135038_IntialCreate.cs` + Designer +
`ApplicationDbContextModelSnapshot.cs` under `service/Api/src/Migrations/Migrations/`)
is removed and a fresh baseline migration is generated from the post-refactor
model. The enum DTO changes introduce no schema delta (entity columns and their
`.HasConversion<string>()` are untouched), so this is a housekeeping squash of
migration history, not a schema migration — performed last so the regenerated
baseline reflects the final model.

## 1. Requirements & Constraints

- **REQ-001**: Every status/state field in a request/response DTO that has a corresponding domain enum must be typed with that enum (not `string`).
- **REQ-002**: The JSON wire format must remain strings — `JsonStringEnumConverter` in `Program.cs` is NOT removed or changed.
- **REQ-003**: Mapping code must assign enum values directly (remove `.ToString()`); request-side `Enum.TryParse` parsing is removed when the DTO field becomes the enum.
- **REQ-004**: New statuses without an existing enum get small PascalCase enums whose member names serialize to the exact strings emitted today (lowest-risk wire preservation).
- **REQ-005**: Both SPAs consume statuses via typed string-literal unions with label + severity maps; no raw untyped `string` status field remains displayed without a map.
- **REQ-006**: Admin SPA wires Capture/Refund/Void buttons (PaymentsList + OrderDetail payments tab) and Mark Shipped / Mark Delivered quick buttons (OrderDetail shipments section) to existing endpoints, gated by the payment/shipment state.
- **REQ-007**: Storefront SPA wires a Pay-now button (outstanding balance) and a Cancel-order button (Placed status) on the order detail page to existing endpoints.
- **REQ-008**: After all enum/DTO changes are implemented, the EF Core migration baseline is rebuilt: the existing `20260816135038_IntialCreate` migration (`.cs` + `.Designer.cs`) and `ApplicationDbContextModelSnapshot.cs` are removed and a single fresh `InitialCreate` migration is generated from the post-refactor model.
- **SEC-001**: No new secrets, PII, or logging of payment data; action buttons reuse existing authenticated/authorized endpoints.
- **CON-001**: `TreatWarningsAsErrors=true` — any warning fails the build; new code must compile clean.
- **CON-002**: Vertical-slice file structure must be preserved; DTO/mapping/validator changes stay in their feature files.
- **CON-003**: The enum/DTO refactor itself requires no schema change (entity columns and their `.HasConversion<string>()` remain untouched); the migration rebuild in Phase 8 is a history squash and must NOT alter the generated schema.
- **CON-004**: Keep the existing `JsonStringEnumConverter` global registration; do not switch to `JsonNumberEnumConverter` or remove it (explicit user decision).
- **CON-005**: Do not alter the `TaxonRule` `[EnumMember(Value = "snake_case")]` wire contract — its DTO fields stay `string`.
- **CON-006**: Migration commands target `--project service/Api/src/Migrations/Api.Migrations.csproj --startup-project service/Api/src/Api/Api.csproj`; generated migration/snapshot files must never be hand-edited (per `service/Api/src/Migrations/GUIDE.yaml` G1/G2).
- **GUD-001**: Follow the existing enum-typed DTO pattern already used by `OrderDetailResponse`/`OrderListItemResponse` (`Ordering/Features/Admin/Shared/Models/Order.Model.cs`).
- **GUD-002**: SPA status maps use `Record<UnionType, severity>` shape as in `OrderDetail.vue` `STATUS_SEVERITY`/`FULFILLMENT_SEVERITY`.
- **PAT-001**: DTO enum field → `JsonStringEnumConverter` serializes the member name; mapping assigns `payment.State` directly (no `.ToString()`).
- **PAT-002**: New enums use PascalCase members that match the current string values so the wire output is byte-identical.

## 2. Implementation Steps

### Implementation Phase 1

- GOAL-001: Convert the Billing payment DTO cluster (17 derived models) to use `PaymentRecordState` for `State` and remove all `.ToString()` emission.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Change `PaymentParameters.State` (`Billing/Features/Admin/Shared/Models/Payment.Model.cs:15`) from `string` to `PaymentRecordState`. `PaymentStatus` (`:16`) stays `string?` (gateway provider text). All inheriting records (`PaymentRequest`, `PaymentDetailResponse`, `PaymentListItemResponse`, `StorePaymentRequest`, `StorePaymentDetailResponse`, `StorePaymentListItemResponse`, and every endpoint `Response` in Admin/Payments/* and Storefront/Payment/*) inherit the change with no edits. | ✅ | 2026-08-17 |
| TASK-002 | Update `PaymentRecordMapping.MapToDetail`/`MapToListItem` (`Billing/Features/Admin/Shared/Mappings/Payment.Mapping.cs:28,48`): `State = payment.State.ToString()` → `State = payment.State`. | ✅ | 2026-08-17 |
| TASK-003 | Update `PaymentStoreMapping.MapToStoreDetail`/`MapToStoreListItem` (`Billing/Features/Storefront/Shared/Mappings/Storefront.Payment.Mapping.cs:28,47`) and `GetPaymentStatus.cs:44`: assign `payment.State` directly (remove `.ToString()`). | ✅ | 2026-08-17 |
| TASK-004 | Build the Module/Api projects; run the Billing unit tests to confirm no regressions (warnings-as-errors clean). | ✅ | 2026-08-17 |

### Implementation Phase 2

- GOAL-002: Convert remaining Ordering status fields (`RecentOrderData.Status`, `OrderTimelineEvent.Type`) to enums and fix their mappings.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | Create `OrderTimelineEventType` enum (PascalCase: `Created, Placed, Approved, PaymentProcessing, PaymentCompleted, PaymentFailed, Shipped, Delivered, Canceled`) in `Ordering/Domain/Orders/` and change `OrderTimelineEvent.Type` (`Ordering/Features/Admin/Shared/Models/Order.Model.cs:163`) from `string` to it. | ✅ | 2026-08-17 |
| TASK-006 | Update `Order.Mapping.BuildTimeline` (`Ordering/Features/Admin/Shared/Mappings/Order.Mapping.cs:190-198`): `Type = OrderTimelineEventType.Created` etc. (replace hardcoded lowercase strings). | ✅ | 2026-08-17 |
| TASK-007 | Change `RecentOrderData.Status` (`Ordering/Features/Admin/Shared/Models/OrderingDashboard.Model.cs:19`) from `string` to `OrderStatus`; update `GetOrderingDashboard.cs:45` to `Status = o.Status` (remove `.ToString()`). | ✅ | 2026-08-17 |
| TASK-008 | Build + run Ordering unit tests. | ✅ | 2026-08-17 |

### Implementation Phase 3

- GOAL-003: Convert Inventory status DTO fields (`StockTransferParameters.State`, `CartReservationStatus.State`, low-stock status) to enums.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Change `StockTransferParameters.State` (`Inventory/Features/Admin/Shared/Models/StockTransfer.Model.cs:12`) from `string?` to `TransferState?`; update `StockTransfer.Mapping.cs:33,56` and `GetStockTransferPagedOrAll.cs:37` to assign `x.State` directly. | ✅ | 2026-08-17 |
| TASK-010 | Change `CartReservationStatus.State` (`Inventory/Features/Storefront/StockReservations/Get/GetCartReservations.Response.cs:13`) from `string` to `ReservationState`; update `GetCartReservations.Endpoint.cs:34` to `r.Reservation.State`. | ✅ | 2026-08-17 |
| TASK-011 | Create `LowStockStatus` enum (`Low`, `OutOfStock`) and change `GetLowStockItems.Response.Status` (`Inventory/Features/Admin/StockItems/LowStock/GetLowStockItems.Response.cs:11`) from the hardcoded string `"low"` to `LowStockStatus.Low`. Note (plan correction): the handler also emits `"out_of_stock"` for zero on-hand, so the enum has two members. | ✅ | 2026-08-17 |
| TASK-012 | Build + run Inventory unit tests. | ✅ | 2026-08-17 |

### Implementation Phase 4

- GOAL-004: Convert Catalog (`VariantImage.Type`, `Variant` units, `Embedding.Status`) and Dashboard (`ActivityItemData`) status DTO fields to enums.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-013 | Change `VariantImageParameters.Type` (`Catalog/Features/Admin/Shared/Models/VariantImage.Model.cs:7`) from `string` to `VariantImageType`; update `VariantImage.Mapping.cs:33,61` (remove `.ToString()`) and the request parsing in `Upload/UploadVariantImage.cs:77` + `Update/UpdateVariantImage.cs:43` (remove `Enum.TryParse`). Note (user decision): `UpdateImageRequest.Type` made nullable (`VariantImageType?`) — null = not provided (preserve existing), `Default` = explicit demote. | ✅ | 2026-08-17 |
| TASK-014 | Change `VariantParameters.WeightUnit`/`DimensionsUnit` (`Catalog/Features/Admin/Shared/Models/Variant.Model.cs:9,13`) from `string?` to `WeightUnit?`/`DimensionUnit?`; update `Variant.Mapping.cs:43-44` (remove `Enum.TryParse`), `Variant.Mapping.cs:82,86,106,110` (remove `.ToString()`), and validators `Variant.Validator.cs:24,30`. | ✅ | 2026-08-17 |
| TASK-015 | Change `EmbeddingDetailResponse.Status` (`Catalog/Features/Admin/Shared/Models/ImageEmbedding.Model.cs:23`) from `string` to `EmbeddingStatus`; update `Get/GetEmbedding.cs:30`, `Create/ImageEmbedding.Create.cs:71`, `Regenerate/ImageEmbedding.Regenerate.cs:77`. | ✅ | 2026-08-17 |
| TASK-016 | Create `ActivityType` (`Order, Stock`) and `ActivityStatus` (`Draft, Placed, Canceled, Expired, Completed`) enums and change `ActivityItemData.Type`/`Status` (`Dashboard/Features/Admin/Shared/Models/Dashboard.Model.cs:55,58`) from `string` to them; update `Get/GetDashboard.cs` (Order branch maps OrderStatus → ActivityStatus in memory after materialization; Stock branch uses `ActivityStatus.Completed`). | ✅ | 2026-08-17 |
| TASK-017 | Build + run Catalog and Dashboard unit tests. | ✅ | 2026-08-17 |

### Implementation Phase 5

- GOAL-005: Admin SPA — typed status unions, correct display maps, and payment/shipment action buttons wired to the API.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-018 | `app/Admin/src/features/payment/types/payment.ts`: add `type PaymentRecordState = 'Checkout' \| 'Processing' \| 'Pending' \| 'Completed' \| 'Failed' \| 'Void' \| 'Disputed' \| 'Invalid'`; type `PaymentListItem.state`, `PaymentDetail.state`, `PaymentQuery.state` with it; add `PAYMENT_STATE_SEVERITY: Record<PaymentRecordState, string>` and a label map. Keep `paymentStatus?: string`. | ✅ | 2026-08-18 |
| TASK-019 | `app/Admin/src/features/ordering/types/order.ts`: type `PaymentCaptureSummary.state` (`:178`) with the new `PaymentRecordState` union; `orderingDashboard.ts`: type `RecentOrderData.status` (`:5`) as `OrderStatus`; `inventory/types/stockItem.ts`: `LowStockItem.status` union → `'Low' \| 'OutOfStock'`. | ✅ | 2026-08-18 |
| TASK-020 | `app/Admin/src/features/ordering/views/OrderDetail.vue` Payments tab (`:482`): render `Tag` with `PAYMENT_STATE_SEVERITY`; add a Row Actions column with Capture (state `Pending`/`Processing`), Refund (state `Completed`), Void (state `Pending`/`Processing`) buttons + ConfirmDialog, wired to `PaymentApi.capturePayment/refundPayment/voidPayment`; reload the order after success. | ✅ | 2026-08-18 |
| TASK-021 | `app/Admin/src/features/payment/views/PaymentsList.vue`: add a Row Actions column with the same Capture/Refund/Void buttons + dialogs (gated by state), wired to `PaymentApi`; call `refresh()` after success; add `PAYMENT_STATE_SEVERITY` to the state `Tag` (`:99`). | ✅ | 2026-08-18 |
| TASK-022 | `app/Admin/src/features/ordering/views/OrderDetail.vue` Shipments section (`:389-431`): add quick-action buttons "Mark Shipped" and "Mark Delivered" per row, wired to `OrderApi.updateShipmentStatus`. Gate by the domain map: Mark Shipped reachable only from Ready (per user decision — Backorder must go Ready first); Mark Delivered only from Shipped; keep the existing dropdown+Save. | ✅ | 2026-08-18 |
| TASK-023 | Run `pnpm run lint` and `pnpm run test:unit` for `app/Admin`; fix any failures (warnings-as-errors lint). | ✅ | 2026-08-18 |

### Implementation Phase 6

- GOAL-006: Storefront SPA — typed status unions, correct display maps, and Pay-now / Cancel-order action buttons.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-024 | `app/Store/src/features/ordering/types/order.ts`: type `PaymentCaptureSummary.state` (`:72`) with a `PaymentRecordState` union; `app/Store/src/features/payment/types/payment.ts`: type `PaymentIntent.state` (`:34`) and `PaymentStatusResponse.state` (`:67`); `app/Store/src/features/ordering/types/checkout.ts`: type `PaymentIntentResponse.state` (`:30`); add severity/label maps for payment states. | ✅ | 2026-08-18 |
| TASK-025 | `app/Store/src/features/ordering/validations/order.ts`: add a `PaymentRecordStateSchema` (`z.enum([...])`) and use it for `PaymentCaptureSummarySchema.state`; `app/Store/src/features/payment/validations/payment.ts`: use it for `PaymentIntentSchema.state`/`PaymentStatusResponseSchema`. | ✅ | 2026-08-18 |
| TASK-026 | `app/Store/src/features/ordering/views/OrderDetailView.vue`: render payment `state` and shipment `status` `Tag`s (`:206,:233`) with severity maps; add a "Pay now" button (status `Placed` and `outstandingBalance > 0`) calling `getPaymentMethods` → `CheckoutApi.createPaymentIntent` → redirect to `checkoutUrl`; add a "Cancel order" button (status `Placed`/`Draft`) calling `orders.cancelOrder`, then refresh. | ✅ | 2026-08-18 |
| TASK-027 | Run `pnpm run lint` and `pnpm run test:unit` for `app/Store`; fix any failures. | ✅ | 2026-08-18 |

### Implementation Phase 7

- GOAL-007: Full verification across backend and both SPAs.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-028 | `dotnet build service/Api/src/Api/Api.csproj` (0 warnings/0 errors) and run the full `Module.UnitTests` suite (only the 3 pre-existing `OrderStatusValueConverterTests` failures expected). | ✅ | 2026-08-18 |
| TASK-029 | Run `bash scripts/check-feature-conventions.sh`; confirm no new drift. | ✅ | 2026-08-18 |

### Implementation Phase 8

- GOAL-008: Remove the existing EF Core migration history and regenerate a single fresh `InitialCreate` baseline from the post-refactor model.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-030 | Delete the existing migration files under `service/Api/src/Migrations/Migrations/`: `20260816135038_IntialCreate.cs`, `20260816135038_IntialCreate.Designer.cs`, and `ApplicationDbContextModelSnapshot.cs`. | ✅ | 2026-08-18 |
| TASK-031 | Generate the fresh baseline: `dotnet ef migrations add InitialCreate --project service/Api/src/Migrations/Api.Migrations.csproj --startup-project service/Api/src/Api/Api.csproj`. Review the generated `.cs` + `.Designer.cs` + snapshot — schema must match the pre-deletion model (no enum-DTO columns change). | ✅ | 2026-08-18 |
| TASK-032 | Confirm the regenerated migration is the only one: `dotnet ef migrations list --project service/Api/src/Migrations/Api.Migrations.csproj --startup-project service/Api/src/Api/Api.csproj` shows exactly one `InitialCreate` entry. | ✅ | 2026-08-18 |
| TASK-033 | Re-run `dotnet build service/Api/src/Api/Api.csproj` (0 warnings/0 errors) and the `Module.UnitTests` suite to confirm the rebuilt migrations compile and nothing regressed. | ✅ | 2026-08-18 |
| TASK-034 | Update this plan's task table (Completed/Date) and set front-matter `status` to `Completed` once all phases pass. | ✅ | 2026-08-18 |

## 3. Alternatives

- **ALT-001**: Switch the global serializer to integers (`JsonNumberEnumConverter`) and type everything numeric. Rejected by the user — they explicitly chose to keep the normal JSON string enum behavior and fix the SPA display mapping instead.
- **ALT-002**: Convert every free-text field (gateway `PaymentStatus`, `StockMovement` Action/Reason/OriginatorType, `AdjustmentSummary.SourceType`, `ShippingMethod.CalculatorType`) to enums too. Rejected — these are provider-specific or free-form audit data; fabricating enums risks corrupting stored values and widening the blast radius with no display benefit.
- **ALT-003**: Convert `TaxonRule.Type`/`MatchPolicy` to enums. Rejected — their `[EnumMember(Value = "snake_case")]` wire contract would break the Admin rule editor unless a custom EnumMember-aware converter is added; left as a documented follow-up.

## 4. Dependencies

- **DEP-001**: Existing domain enums (`PaymentRecordState`, `TransferState`, `ReservationState`, `VariantImageType`, `WeightUnit`, `DimensionUnit`, `EmbeddingStatus`, `OrderStatus`, `ShipmentStatus`) — all already exist; no new packages.
- **DEP-002**: Global `JsonStringEnumConverter` (`service/Api/src/Api/Program.cs:31-33`) — must remain registered; wire format unchanged.
- **DEP-003**: Existing action endpoints: `POST /api/admin/billing/payments/{id}/capture|refund|void`, `PUT /api/admin/shipping/shipments/{id}/status`, `POST /api/storefront/orders/{id}/cancel`, `POST /api/storefront/cart/payment/intent` — already implemented; only UI wiring is new.
- **DEP-004**: Admin SPA `PaymentApi` (`app/Admin/src/features/payment/services/paymentApi.ts:32-42`) — Capture/Refund/Void functions already exist and are currently unused by any view; they become the wiring targets.
- **DEP-005**: `dotnet-ef` CLI tool (`dotnet tool install --global dotnet-ef`) and the `Microsoft.EntityFrameworkCore.Design` package (already referenced in `Api.Migrations.csproj:16`); required for Phase 8 migration rebuild.

## 5. Files

- **FILE-001**: `service/Api/src/Module/Billing/Features/Admin/Shared/Models/Payment.Model.cs` — `PaymentParameters.State` → `PaymentRecordState`.
- **FILE-002**: `service/Api/src/Module/Billing/Features/Admin/Shared/Mappings/Payment.Mapping.cs` — remove `.ToString()` (lines 28, 48).
- **FILE-003**: `service/Api/src/Module/Billing/Features/Storefront/Shared/Mappings/Storefront.Payment.Mapping.cs` — remove `.ToString()` (lines 28, 47).
- **FILE-004**: `service/Api/src/Module/Billing/Features/Storefront/Payment/Status/GetPaymentStatus.cs` — line 44.
- **FILE-005**: `service/Api/src/Module/Ordering/Domain/Orders/Order.Enumerate.cs` — add `OrderTimelineEventType`.
- **FILE-006**: `service/Api/src/Module/Ordering/Features/Admin/Shared/Models/Order.Model.cs` — `OrderTimelineEvent.Type` and `PaymentCaptureSummary` (line 138 already enum; line 139 `PaymentStatus` stays string).
- **FILE-007**: `service/Api/src/Module/Ordering/Features/Admin/Shared/Mappings/Order.Mapping.cs` — `BuildTimeline` enum assignment.
- **FILE-008**: `service/Api/src/Module/Ordering/Features/Admin/Shared/Models/OrderingDashboard.Model.cs` + `Dashboard/Get/GetOrderingDashboard.cs` — `RecentOrderData.Status`.
- **FILE-009**: `service/Api/src/Module/Inventory/Features/Admin/Shared/Models/StockTransfer.Model.cs` + `Mappings/StockTransfer.Mapping.cs` + `StockTransfers/Get/Paged/GetStockTransferPagedOrAll.cs`.
- **FILE-010**: `service/Api/src/Module/Inventory/Features/Storefront/StockReservations/Get/GetCartReservations.Response.cs` + `Endpoint.cs`.
- **FILE-011**: `service/Api/src/Module/Inventory/Features/Admin/StockItems/LowStock/GetLowStockItems.Response.cs` + `.Enumerate.cs` — new `LowStockStatus` enum.
- **FILE-012**: `service/Api/src/Module/Catalog/Features/Admin/Shared/Models/VariantImage.Model.cs` + `Mappings/VariantImage.Mapping.cs` + `Upload/UploadVariantImage.cs` + `Update/UpdateVariantImage.cs`.
- **FILE-013**: `service/Api/src/Module/Catalog/Features/Admin/Shared/Models/Variant.Model.cs` + `Mappings/Variant.Mapping.cs` + `Shared/Validators/Variant.Validator.cs`.
- **FILE-014**: `service/Api/src/Module/Catalog/Features/Admin/Shared/Models/ImageEmbedding.Model.cs` + embedding Get/Create/Regenerate handlers.
- **FILE-015**: `service/Api/src/Module/Dashboard/Features/Admin/Shared/Models/Dashboard.Model.cs` + `Get/GetDashboard.cs` — new `ActivityType`/`ActivityStatus`.
- **FILE-016**: `app/Admin/src/features/payment/types/payment.ts` + `views/PaymentsList.vue` — unions, maps, action buttons.
- **FILE-017**: `app/Admin/src/features/ordering/types/order.ts` + `orderingDashboard.ts` + `views/OrderDetail.vue` — unions, maps, payment + shipment actions.
- **FILE-018**: `app/Store/src/features/ordering/types/order.ts` + `views/OrderDetailView.vue` — unions, maps, Pay-now / Cancel buttons.
- **FILE-019**: `app/Store/src/features/ordering/validations/order.ts` + `app/Store/src/features/payment/validations/payment.ts` — zod schemas.
- **FILE-020**: `service/Api/src/Migrations/Migrations/20260816135038_IntialCreate.cs` + `.Designer.cs` — existing migration, deleted in Phase 8 (TASK-030).
- **FILE-021**: `service/Api/src/Migrations/Migrations/ApplicationDbContextModelSnapshot.cs` — existing snapshot, deleted and regenerated in Phase 8.
- **FILE-022**: `service/Api/src/Migrations/Migrations/{timestamp}_InitialCreate.cs` + `.Designer.cs` — fresh baseline migration generated by TASK-031.
- **FILE-023**: `service/Api/tests/Module.UnitTests/.../EnumWireFormatSerializationTests.cs` — new wire-format serialization guards (added in final fix wave per TEST-002).

## 6. Testing

- **TEST-001**: Backend unit tests assert mapped DTOs carry the enum value (e.g. `PaymentDetailResponse.State == PaymentRecordState.Pending`) — extend existing Billing mapping tests.
- **TEST-002**: Backend serialization tests assert `JsonSerializer` emits the enum member name string for the new/typed fields (wire-format regression guard) — delivered in the final fix wave (`EnumWireFormatSerializationTests` covering OrderTimelineEvent.Type, PaymentRecordState, LowStockStatus).
- **TEST-003**: Admin SPA unit tests: `PaymentsList`/`OrderDetail` render the correct severity Tag per payment state and the Capture/Refund/Void buttons are present/disabled per state.
- **TEST-004**: Admin SPA unit tests: shipment quick-action buttons call `OrderApi.updateShipmentStatus` with the right payload and refresh (including Backorder/Pending disabled gates).
- **TEST-005**: Storefront SPA unit tests: Pay-now and Cancel-order buttons call the right API functions and update the view.
- **TEST-006**: Storefront zod validation tests cover the new `PaymentRecordStateSchema`.
- **TEST-007**: Phase 8 migration check: `dotnet ef migrations list` returns exactly one `InitialCreate` and the generated snapshot matches the pre-deletion model (no unintended schema delta introduced by the enum refactor).

## 7. Risks & Assumptions

- **RISK-001**: Any SPA code comparing statuses by string could silently match the wrong union if a member name differs from the wire value — mitigated by PAT-002 (member names == current emitted strings) and the full SPA test suites.
- **RISK-002**: Changing `OrderTimelineEvent.Type` from lowercase (`"created"`) to PascalCase (`"Created"`) alters the wire value for that field — verified safe because both SPAs render `label`, never `type`; documented so future consumers expect PascalCase.
- **RISK-003**: The `TaxonRule` contract is intentionally left as strings — if a future requirement needs enum typing there, a custom EnumMember-aware JSON converter is required (see ALT-003).
- **RISK-004**: Removing the migration files and regenerating a new baseline is a destructive history squash — if the old `20260816135038_IntialCreate` has already been applied to a non-local database, environments must be rebuilt from scratch (drop + re-migrate). Documented and scoped to local/dev in ASSUMPTION-004.
- **ASSUMPTION-001**: Enum member names match the strings currently produced by `.ToString()` on the domain enums (verified for the converted set).
- **ASSUMPTION-002**: No client sends a status string that is not a valid enum member name (they come from the API itself), so request binding remains lossless.
- **ASSUMPTION-003**: The two legacy SPAs (`app/legacy/*`) are out of scope and may keep string-based status handling.
- **ASSUMPTION-004**: The single existing migration has not been applied to any production/shared database, so deleting and regenerating it (Phase 8) is safe; local dev databases can be dropped and re-migrated.

## 8. Related Specifications / Further Reading

- [Codebase architecture](./codebase/ARCHITECTURE.md)
- [Domain principles](./../.harness/principles.yml)
- [Enforcement rules](./../.harness/enforcement.yml)
- [Order enrichment design (timeline statuses)](../docs/superpowers/specs/2026-08-16-order-details-enrichment-design.md)
- [Cross-module state sync design](../docs/superpowers/specs/2026-08-16-order-cross-module-state-sync-design.md)
- [EF Core migration guide](service/Api/src/Migrations/GUIDE.yaml)